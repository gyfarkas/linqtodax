using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdventureWorks;
using FluentAssertions;
using LinqToDAX;
using LinqToDAX.Query;
using NUnit.Framework;
using System;
using System.Linq;
using TabularEntities;

using Customer = AdventureWorks.Customer;
namespace UnitTests
{
    [TabularTableMapping("'Internet Sales'")]
    public class InternetSalesTest : ITabularData
    {
        public Customer Customer { get; set; }
    }

    public class DummyTest
    {
        public String SomeString { get; set; }
        public long SomeInt { get; set; }
    }

    [TestFixture]
    public class QueryTests
    {
        private const string ConnectionString =
            "Provider=MSOLAP;Data Source=LDDEVCUBEDB2;Initial Catalog=AdventureWorks Tabular Model SQL 2012;";

        private readonly AdventureWorksContext _db;

        public QueryTests()
        {
            _db = new AdventureWorksContext(ConnectionString);
        }

        [Test]
        public void SimpleQuery()
        {
            var table = _db.CustomerSet;
            var titles =
                from customer in table
                select customer.Title;

            var res = titles.ToList();
            res.Should().Contain("Mrs.");

        }

        [Test]
        public void SimpleQuery_withProjection()
        {

            var titles =
                from customer in _db.CustomerSet
                where customer.TotalCarsOwned > 0 && customer.Gender == "F"
                select new
                {
                    Orders = customer.Internet_Distinct_Count_Sales_Order(),
                    Name = customer.FirstName + " " + customer.LastName,
                    Cars = customer.TotalCarsOwned,
                    Sales = customer.Internet_Total_Sales()
                };

            var res = titles.ToList();
            res.Select(x => x.Cars).Should().Contain(3);
        }


        [Test]
        public void SimpleQuery_WithWhere()
        {

            var titles =
                from customer in _db.CustomerSet
                where customer.Gender == "M" && customer.Title != ""
                select customer.Title;
            var res = titles.ToList();
            res.Should().NotContain("Mrs.");
        }


        [Test]
        public void SimpleQuery_OverMoreTables()
        {

            const int custno = 20929;
            var q =
                from geo in _db.GeographySet
                // these tables are used 
                from customer in _db.CustomerSet
                // for referencing attributes
                from sales in _db.InternetSalesSet
                // the last table listed gets into the summarize
                /*
                 * better would be to encode the relationships in the entities and have InternetSales.RelatedCustomer 
                 * 
                 */
                where sales.RelatedCustomer.CustomerKey == custno
                select new
                {
                    Name = customer.LastName + " " + customer.FirstName,

                    City = new
                    {
                        CityName = geo.City,
                        Country = geo.CountryRegionName,
                    },
                    Discount = sales.DiscountAmount,
                    Date = sales.OrderDate,
                    Sales = sales.Internet_Total_Sales()
                };
            var res = q.ToList();

            res.Select(x => x.City.CityName).Should().Contain("York");
        }

        [Test]
        public void SimpleQuery_WithTopnImplicitOrdering()
        {

            var query =
                (

                    from sales in _db.InternetSalesSet
                    where sales.RelatedCustomer.RelatedGeography.CountryRegionCode == "GB"
                    select new
                    {
                        Person = new
                        {
                            Full = sales.RelatedCustomer.FirstName + " " + sales.RelatedCustomer.LastName,
                            First = sales.RelatedCustomer.FirstName,
                            Last = sales.RelatedCustomer.LastName,
                            sales.RelatedCustomer.RelatedGeography.City
                        },
                        Data = new
                        {
                            Sales = sales.Internet_Total_Sales(),
                            Orders = sales.Internet_Distinct_Count_Sales_Order()
                        }
                    }
                    ).Take(10);

            var res = query.ToList();
            res.Should().HaveCount(10);
        }

        [Test]
        public void SimpleQuery_FilterOnMeasure()
        {

            var query =
                from customer in _db.CustomerSet
                from sales in _db.InternetSalesSet
                where sales.Internet_Total_Sales() > 0
                select new
                {
                    Profit = customer.Total_Gross_Profit(),
                    Name = customer.LastName
                };
            var result = query.Take(10).ToList().Select(x => x.Name);
            result.Should().Contain("Xu");
        }

        [Test]
        public void CalculateTable()
        {

            const string xu = "Xu";
            var q1 =
                from customer in _db.CustomerSet
                where customer.LastName == xu && customer.FirstName == "Tony"
                select customer.CustomerId;

            var q2 =
                from x in _db.InternetSalesSet
                select new
                {
                    x.RelatedCustomer.CustomerId,
                    Sales = x.InternetTotalSales(),
                    Orders = x.InternetOrderLinesCount()
                };

            var result = q2.CalculateTable(q1).ToList().Select(r => Math.Round(r.Sales, 0));

            result.Should().Contain(2722);
        }

        [Test]
        public void CalculatetableSeries()
        {
            var q1 =
                from customer in _db.CustomerSet
                where customer.LastName == "Xu" && customer.FirstName == "Tony"
                select customer.CustomerId;

            var q12 =
                from geo in _db.GeographySet
                where geo.City == "London"
                select geo.GeographyKey;

            var q2 =
                from x in _db.InternetSalesSet
                select new
                {
                    x.RelatedCustomer.CustomerId,
                    x.RelatedCustomer.GeographyKey
                };
            var res = q2.CalculateTable(q12.CalculateTable(q1)).CountRows();

            res.Should().BeGreaterThan(0);

        }

        [Test]
        public void SumTest()
        {
            string xu = "Xu";

            var q =
                from sales in _db.InternetSalesSet
                // this ternary operator is eliminated the test can be evaluated before the query
                // this form can be used to add conditional filters to a query
                where ((xu == null) ? true : sales.RelatedCustomer.LastName == xu)
                      && sales.InternetTotalSales() > 0
                select new
                {
                    Name = sales.RelatedCustomer.FirstName + " " + sales.RelatedCustomer.LastName,
                    sales.RelatedCustomer.RelatedGeography.City,
                    Sum = sales.SalesAmount.Sum()
                };
            var result = q.Take(10).ToList();
            Assert.IsNotEmpty(result);
            result.Should().HaveCount(10);

        }

        [Test]
        public void SumFiltered()
        {

            var tony = "Tony";
            var q =
                (from c in _db.CustomerSet
                    from sales in _db.InternetSalesSet
                    select
                        new
                        {
                            constant = "Tony",
                            Sum = sales.SalesAmount.Sum(c.FirstName == tony)
                        }).ToList();

            const decimal sumOfTonys = (decimal) 49070.2846;
            Assert.AreEqual(q.First().Sum, sumOfTonys);

        }

        [Test]
        public void SumAll()
        {

            var q =
                (
                    from sales in _db.InternetSalesSet
                    from customer in _db.CustomerSet
                    where sales.RelatedCustomer.Gender == "M"
                    select
                        new
                        {
                            constant = "Total",
                            Email = GetLength(customer.EmailAddress),
                            Name = customer.FirstName + " " + customer.LastName,
                            Sum = sales.SalesAmount.Sum(customer.ForAll() && customer.RelatedGeography.ForAll())
                        }
                    ).ToList();
            const decimal sumOfTonys = (decimal) 49070.2846;
            q.First().Sum.Should().BeGreaterThan(sumOfTonys);
        }

        private int GetLength(string p)
        {
            return p.ToCharArray().Count();
        }

        [Test]
        public void SumAllSelected()
        {
            var q =
                (from c in _db.CustomerSet
                    from sales in _db.InternetSalesSet
                    where c.FirstName == "Tony"
                    select
                        new
                        {
                            constant = "SubTotal",
                            Name = c.FirstName,
                            Sum = sales.SalesAmount.Sum(c.FirstName.ForAllSelected()),
                            TotalNumberOfCustomerIds = c.CustomerId.DistinctCount(c.ForAll()),
                            Something = c.Internet_Current_Quarter_Sales(c.ForAll())
                        }
                    ).ToList();
            const decimal sumOfTonys = (decimal) 49070.2846;
            Assert.AreEqual(q.First().Sum, sumOfTonys);
            q.First().TotalNumberOfCustomerIds.Should().Be(18484);
        }

        [Test]
        public void LookupValue()
        {

            var q =
                (from geo in _db.GeographySet
                    from customer in _db.CustomerSet
                    select new
                    {
                        Key = customer.GeographyKey,
                        E = customer.TotalCarsOwned.Max(customer.ForAll()),
                        Value = geo.City.LookupValue(geo.GeographyKey, customer.GeographyKey),

                    }).Where(x => x.Value == "London");
            q.ToList().First().Value.Should().Be("London");
        }

        [Test]
        public void MemberInitTest()
        {
            var tony = "Tony";

            var q =
                from customer in _db.CustomerSet
                where customer.FirstName == "Tony"
                select
                    new
                    {
                        Tony = "tony",
                        Test = new DummyTest
                        {
                            SomeInt = customer.TotalCarsOwned.Min(customer.FirstName == tony),
                            SomeString = customer.LastName
                        }

                    };
            var result = q.ToList();
            var first = result.FirstOrDefault();
            if (first != null)
            {
                first.Test.Should().BeOfType<DummyTest>();
            }

            result.Select(x => x.Test.SomeString)
                .Should().Contain("Xu");
        }

        [Test]
        public void RelatedDistinctCountPattern()
        {

            var q =
                from sales in _db.InternetSalesSet
                select new
                {
                    Year = sales.RelatedDate.CalendarYear,
                    SoldProduct = sales.RelatedProduct.ProductName.DistinctCount(sales),
                    SoldProductCategory = sales.RelatedProduct.ProductCategoryName.DistinctCount(sales)
                };
            var res = q.ToList().First(r => r.Year == 2008);
            res.Should().NotBeNull();
            res.SoldProduct.Should().Be(102);
            res.SoldProductCategory.Should().Be(3);
        }

        [Test]
        public void CumulativeTotalPattern()
        {

            var q =
                from sales in _db.InternetSalesSet
                select new
                {
                    Year = sales.RelatedDate.CalendarYear,
                    Sum = sales.SalesAmount.Sum(),
                    CumulativeSum =
                        sales.SalesAmount.Sum(
                            sales.ForAll()
                            &&
                            sales.RelatedDate.ForAll(
                                sales.RelatedDate.Date2 <= sales.RelatedDate.Date2.Max()))
                };
            var result = q.ToList();
            var expected = result[0].CumulativeSum + result[1].Sum;
            result[1].CumulativeSum.Should().Be(expected);
            result[1].CumulativeSum.Should().BeGreaterThan(result[0].CumulativeSum);
        }

        [Test]
        public void TopnTest()
        {
            var q =
                (from x in _db.InternetSalesSet
                    select x.SalesAmount).Take(10);
            var result = q.ToList();
            result.Should().NotBeNull();
        }

        [Test]
        public void SumxTest()
        {
            var q =
                from dates in _db.DateSet
                select new
                {
                    dates.CalendarYear,
                    sum = (
                        from x in _db.InternetSalesSet
                        where x.RelatedCustomer.RelatedGeography.City == "London"
                        select
                            new
                            {
                                x.RelatedCustomer.CustomerId,
                                v = x.SalesAmount.Sum()
                            }).Take(10).Sumx(x => x.v)
                };
            var result = q.ToList();

            result.Should().NotBeNull();
        }


        [Test]
        public void SumxStandalone()
        {
            var s = (from x in _db.InternetSalesSet
                where x.RelatedCustomer.RelatedGeography.City == "London"
                select
                    new
                    {
                        x.RelatedCustomer.CustomerId,
                        v1 = x.InternetTotalSales(),
                        v = x.SalesAmount.Sum()
                    }).Take(10).Sumx(x => x.v1 + x.v);
            s.Should().NotBe(null);
        }

        public class  BaseForAverage 
        {
            public string x { get; set; }
            public decimal v { get; set; }
        }

        [Test]
        public async Task SumxStandaloneAsync()
        {
            var s = await (from x in _db.InternetSalesSet
                     where x.RelatedCustomer.RelatedGeography.City == "London"
                     select
                         new BaseForAverage
                         {
                             x = x.RelatedCustomer.CustomerId,
                             v = x.SalesAmount.Sum()
                         }).Take(10).SumxAsync(x => x.v);
            
            s.Should().NotBe(null);
        }

        [Test]
        public void AverageXStandalone()
        {
            var s = (from x in _db.InternetSalesSet
                where x.RelatedCustomer.RelatedGeography.City == "London"
                select
                    new BaseForAverage
                    {
                        x = x.RelatedCustomer.CustomerId,
                        v = x.SalesAmount.Sum()
                    }).Take(10).Averagex(x => x.v);
            s.Should().NotBe(null);
        }

        [Test]
        public void AverageXStandaloneAsync()
        {
            var s = (from x in _db.InternetSalesSet
                     where x.RelatedCustomer.RelatedGeography.City == "London"
                     select
                         new
                         {
                             x.RelatedCustomer.CustomerId,
                             v = x.SalesAmount.Sum()
                         }).Take(10).AveragexAsync(x => x.v);
            s.Wait();
            s.Result.Should().NotBe(null);
        }

        [Test]
        public void MinXStandalone()
        {
            var city = "London";
            var s = (from x in _db.InternetSalesSet
                where x.RelatedCustomer.RelatedGeography.City == city
                select
                    new
                    {
                        x.RelatedCustomer.CustomerId,
                        v = x.SalesAmount.Sum()
                    }).Take(10).Minx(x => x.v);
            s.Should().NotBe(null);
        }

        [Test]
        public void MinXStandaloneAsync()
        {
            var city = "London";
            var s = (from x in _db.InternetSalesSet
                     where x.RelatedCustomer.RelatedGeography.City == city
                     select
                         new
                         {
                             x.RelatedCustomer.CustomerId,
                             v = x.SalesAmount.Sum()
                         }).Take(10).MinxAsync(x => x.v);
            s.Wait();
            s.Result.Should().NotBe(null);
        }

        [Test]
        public void MaxXStandalone()
        {
            var s = (from x in _db.InternetSalesSet
                where x.RelatedCustomer.RelatedGeography.City == "London"
                select
                    new
                    {
                        x.RelatedCustomer.CustomerId,
                        v = x.SalesAmount.Sum()
                    }).Take(10).Maxx(x => x.v);
            s.Should().NotBe(null);
        }

        [Test]
        public void MaxXStandaloneAsync()
        {
            var s = (from x in _db.InternetSalesSet
                     where x.RelatedCustomer.RelatedGeography.City == "London"
                     select
                         new
                         {
                             x.RelatedCustomer.CustomerId,
                             v = x.SalesAmount.Sum()
                         }).Take(10).MaxxAsync(x => x.v);
            s.Wait();
            s.Result.Should().NotBe(null);
        }



        [Test]
        public void RankTest()
        {
            var q =
                (from sales in _db.InternetSalesSet
                    from geo in _db.GeographySet
                    select new
                    {
                        geo.City,
                        Sales = sales.InternetTotalSales(),
                        SalesRank = (
                            from geo1 in _db.GeographySet
                            where geo1.ForAll()
                            select
                                new
                                {
                                    geo1.City,
                                    Sales = sales.InternetTotalSales()
                                }).Rankx(x => x.Sales)
                    }).Take(10);
            ((TabularQueryProvider) q.Provider).Log += Console.WriteLine;
            var result = q.ToList().OrderBy(x => x.SalesRank);
            result.First().SalesRank.Should().Be(1);
        }

        [Test]
        public void FilterAndCalculateTable()
        {
            var q =
                (from sales in _db.InternetSalesSet
                    where sales.RelatedCustomer.OwnsHouse
                    select new
                    {
                        Name = sales.RelatedCustomer.LastName,
                        sales.RelatedCustomer.RelatedGeography.City,
                        Sales = (
                            from x in _db.InternetSalesSet
                            where x.RelatedCustomer.RelatedGeography.City == "London"
                            select
                                new
                                {
                                    x.RelatedCustomer.CustomerId,
                                    v = x.SalesAmount.Sum()
                                }).Take(10).Sumx(x => x.v)
                    }
                    ).Where(x => x.Sales != null);
            var result = q.ToList();
            result.Should().NotBeNull();
        }

        [Test]
        public void RelatedTableAccess()
        {
            var q =
                from prod in _db.ProductCategorySet
                from sales in _db.InternetSalesSet
                where sales.RelatedCustomer.LastName == "Xu"
                select new
                {
                    CustomerName = sales.RelatedCustomer.FirstName + " " + sales.RelatedCustomer.LastName,
                    Geo = sales.RelatedCustomer.RelatedGeography
                };
            var result = q.ToList();
            result.Should().NotBeNull();


        }

        [Test]
        public void GenerateTest()
        {
            var h = "hello";
            var w = "World";
            var a = new[] {1, 2, 3};
            var q =
                from c in _db.SalesTerritorySet
                select new {c.SalesTerritoryGroup};

            var q2 =
                from sales in _db.ProductCategorySet
                select new
                {
                    Cat = sales.ProductCategoryName,
                    sum = (from s in _db.ResellerSalesSet select new {s.SalesAmount}).Sumx(x => x.SalesAmount)
                };
            var result =
                q.Generate(q2, (x, y) => new {x.SalesTerritoryGroup, y.Cat, y.sum, Message = h + w, Array = a})
                    .Take(3)
                    .ToList();
            result.Should().NotBeNull();

        }

        [Test]
        public void CalculateTableGenerateTest()
        {
            var q =
                from c in _db.SalesTerritorySet
                select c.SalesTerritoryGroup;
            var filter =
                from c in _db.SalesTerritorySet
                //where c.SalesTerritoryCountry == "USA"
                select c.SalesTerritoryGroup;
            var q2 =
                from sales in _db.ProductCategorySet
                select new Something
                {
                    Cat = sales.ProductCategoryName,
                    sum = (from s in _db.ResellerSalesSet select new {s.SalesAmount}).Sumx(x => x.SalesAmount)
                };
            var result = q.CalculateTable(filter)
                .Generate(q2,
                    (x, y) => new Other {List = new List<string> {x, y.Cat}, Sum = y.sum, Test = y.Cat == "not"})
                .Take(3);
            var temp = result.ToList();
            temp.Should().NotBeNull();
        }

        [Test]
        public void DateTest()
        {
            var q =
                from d in _db.DateSet
                where d.Date2 >= new DateTime(2008, 1, 1) && d.Date2 <= new DateTime(2008, 2, 1)
                select d;
            var result = q.ToList();
            result.Should().NotBeEmpty();
        }

        [Test]
        public void SimpleQuery_FilterOnMeasureAndColumn()
        {

            var query =
                from customer in _db.CustomerSet
                from sales in _db.InternetSalesSet
                where sales.SalesAmount > 0 && sales.Internet_Total_Sales() > 0
                select new
                {
                    Profit = customer.Total_Gross_Profit(),
                    Name = customer.LastName
                };
            var result = query.Take(10).ToList().Select(x => x.Name);
            result.Should().Contain("Xu");
        }

        [Test]
        public void DisjunctiveFilters()
        {
            var query =
                from sales in _db.InternetSalesSet
                where (sales.RelatedCurrency.CurrencyCode == "USD" || sales.RelatedCustomer.LastName == "Xu")
                select new
                {
                    sales.RelatedCurrency.CurrencyCode,
                    sales.RelatedCustomer.LastName
                };
            var result = query.ToList();
            result.Should().Contain(new {CurrencyCode = "USD", LastName = "Yang"});
        }

        [Test]
        public void FilterSeparationTest()
        {
            var query =
                from customer in _db.CustomerSet
                from sales in _db.InternetSalesSet
                where
                    (customer.LastName == "Smith" || customer.LastName == "Smith" || customer.LastName == "Yang" ||
                     "Xu" == customer.LastName) &&
                    (sales.RelatedCustomer.LastName == "Smith" || sales.RelatedCurrency.CurrencyCode == "USD" ||
                     sales.RelatedCustomer.LastName == "Xu") &&
                    (sales.RelatedCurrency.CurrencyCode == "USD" || sales.RelatedCustomer.LastName == "Xu") &&
                    (sales.RelatedCustomer.LastName == "Yang" || "Xu" == sales.RelatedCustomer.LastName)
                    && sales.InternetTotalSales() > 0
                select new
                {
                    sales.RelatedCurrency.CurrencyCode,
                    sales.RelatedCustomer.LastName
                };

            var result = query.ToList();
            result.Should().Contain(new {CurrencyCode = "USD", LastName = "Yang"});
        }

        [Test]
        public void UseRelationshipTest()
        {
            var query =
                from date in _db.DateSet
                from sales in _db.InternetSalesSet
                where
                    (sales.RelatedCurrency.CurrencyCode == "USD" || sales.RelatedCustomer.LastName == "Xu") &&
                    (sales.RelatedCustomer.LastName == "Yang" || "Xu" == sales.RelatedCustomer.LastName) &&
                    sales.InternetTotalSales() > 0
                    && date.DateKey.UseRelationship(sales.ShipDateKey)
                select new
                {
                    sales.RelatedCurrency.CurrencyCode,
                    sales.RelatedCustomer.LastName
                };
            var result = query.ToList();
            result.Should().Contain(new {CurrencyCode = "USD", LastName = "Yang"});
        }

        [Test]
        public void ApplyRelationshipTest()
        {
            var query =
                from date in _db.DateSet
                from sales in _db.InternetSalesSet
                where
                    (sales.RelatedCurrency.CurrencyCode == "USD" || sales.RelatedCustomer.LastName == "Xu") &&
                    (sales.RelatedCustomer.LastName == "Yang" || "Xu" == sales.RelatedCustomer.LastName) &&
                    sales.InternetTotalSales() > 0
                    && date.DateKey.ApplyRelationship("'Internet Sales'[ShipDateKey]")
                select new
                {
                    sales.RelatedCurrency.CurrencyCode,
                    sales.RelatedCustomer.LastName
                };
            var result = query.ToList();
            result.Should().Contain(new {CurrencyCode = "USD", LastName = "Yang"});
        }

        [Test]
        public void CheckDateTest()
        {
            var query =
                from sales in _db.InternetSalesSet
                where sales.DueDate > new System.DateTime(2010, 05, 25)
                select new
                {
                    sales.RelatedCurrency.CurrencyCode,
                    sales.RelatedCustomer.LastName
                };

            Logger checkDate = (msg => msg.Should().Contain("2010-05-25"));
            ((TabularQueryProvider) query.Provider).Log += checkDate;
            var result = query.ToList();
            ((TabularQueryProvider) query.Provider).Log -= checkDate;
            result.Should().NotBeNull();
        }

        [Test]
        public void EmbeddedQureyTest()
        {
            var query =
                from sales in _db.InternetSalesSet
                where sales.RelatedCustomer.LastName == "Xu" || sales.RelatedCustomer.LastName == "Smith"
                let n = sales.RelatedCustomer.LastName
                select new
                {
                    Name = sales.RelatedCustomer.LastName,
                    Customers = (from customer in _db.CustomerSet
                                 where customer.LastName == n
                        select new
                        {
                            Name = customer.FirstName,
                            Value = 1
                        }),
                    //MainName = sales.RelatedCustomer.FirstName
                };
            var res = query.ToArray();
            res.Should().NotBeNull();
        }

        [Test]
        public void ComplexFilterCondition()
        {
            var query =
                from sales in _db.InternetSalesSet
                select sales.ProductKey.DistinctCount(
                    sales.RelatedDate.ForAll(
                        sales.RelatedDate.Date2 <
                        (from s in _db.InternetSalesSet
                            where s.ProductKey.ForAll()
                            select new
                            {
                                s.ProductKey,
                                s.DueDate
                            }).Maxx(x => x.DueDate))
                    );
            var res = query.ToList().FirstOrDefault();
            Assert.IsNotNull(res);
            res.Should().Be(158);
        }

        [Test]
        public void ListInitTest()
        {
            var query =
                from sales in _db.InternetSalesSet
                select new
                {
                    List = new List<string>
                    {
                        sales.RelatedCustomer.FirstName,
                        sales.RelatedCustomer.LastName
                    }
                };
            var result = query.ToList().FirstOrDefault();
            if (result != null)
            {
                result.List.Should().NotBeEmpty();
            }
            if (result == null)
            {
                Assert.Fail("result should not be null");
            }
        }

        [Test]
        public void CountrowsTest()
        {
            var xu = "Xu";
            var q =
                from sales in _db.InternetSalesSet
                select new
                {
                    C = (from customer in _db.CustomerSet
                        where customer.LastName == xu
                        select customer.CustomerKey).CountRows()
                };
            var res = q.ToList();

            var count = (from customer in _db.CustomerSet
                where customer.LastName == xu
                select customer.CustomerKey).CountRows();

            res.Should().NotBeNull();
            count.Should().Be(res.First().C);
        }


        [Test]
        public void CountrowsAsyncTest()
        {
            var xu = "Xu";
            var q =
                from sales in _db.InternetSalesSet
                select new
                {
                    C = (from customer in _db.CustomerSet
                         where customer.LastName == xu
                         select customer.CustomerKey).CountRows()
                };
            var res = q.ToListAsync();

            var count = (from customer in _db.CustomerSet
                         where customer.LastName == xu
                         select customer.CustomerKey).CountRowsAsync();
            Task.WaitAll(new Task[]{ res, count });
            res.Result.Should().NotBeNull();
            count.Result.Should().Be(res.Result.First().C);
        }


        [Test]
        public void CountXTest()
        {
            var q =
                (from customer in _db.CustomerSet select new {customer.LastName, customer.FirstName}).Countx(
                    x => x.LastName);
            q.Should().NotBe(null);
        }

        [Test]
        public void CountXAsyncTest()
        {
            var q =
                (from customer in _db.CustomerSet select new { customer.LastName, customer.FirstName }).CountxAsync(
                    x => x.LastName);
            q.Wait();

            q.Result.Should().NotBe(null);
        }


        [Test]
        public void OrderbyTest()
        {
            var q =
                from customer in _db.CustomerSet
                orderby customer.LastName, customer.MaritalStatus
                select new
                {
                    customer.LastName,
                    customer.FirstName,
                    Sum = customer.Internet_Total_Sales()
                };
            var res = q.ToList();
                res.Should()
                .NotBeNull();

        }

        [Test]
        public void AsyncTest()
        {
            var q =
                from customer in _db.CustomerSet
                select new
                {
                    customer.LastName,
                    customer.FirstName,
                    Sum = customer.Internet_Total_Sales()
                };
            var s = new CancellationTokenSource();
            var t = s.Token;
            var t1 = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1);
                s.Cancel();
            });
            var l = q.CountRowsAsync(t);
            
            try
            {
               Task.WaitAll(new[] {t1,l}, t);
            }
            catch (OperationCanceledException) { }
            catch (AggregateException) { }

            
            l.IsCanceled.Should().Be(true);
        }
    }
}
