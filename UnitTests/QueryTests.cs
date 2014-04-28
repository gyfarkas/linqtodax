using System.Collections.Generic;
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
                from geo in _db.GeographySet         // these tables are used 
                from customer in _db.CustomerSet     // for referencing attributes
                from sales in _db.InternetSalesSet // the last table listed gets into the summarize
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

            const decimal sumOfTonys = (decimal)49070.2846;
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
            const decimal sumOfTonys = (decimal)49070.2846;
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
            const decimal sumOfTonys = (decimal)49070.2846;
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
                select new DummyTest
                {
                    SomeInt = customer.TotalCarsOwned.Min(customer.FirstName == tony),
                    SomeString = customer.LastName
                };
            var result = q.ToList();
            result.Should().BeOfType<List<DummyTest>>();
            result.Select(x => x.SomeString)
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
            var q =
                from c in _db.SalesTerritorySet
                select new { c.SalesTerritoryGroup };

            var q2 =
                from sales in _db.ProductCategorySet
                select new
                {
                    Cat = sales.ProductCategoryName,
                    sum = (from s in _db.ResellerSalesSet select new { s.SalesAmount }).Sumx(x => x.SalesAmount)
                };
            var result = q.Generate(q2, (x, y) => new { x.SalesTerritoryGroup, y.Cat, y.sum }).Take(3);
            result.ToList().Should().NotBeNull();

        }

        [Test]
        public void DateTest()
        {
            var q =
                from d in _db.DateSet
                where d.Date2 >= new DateTime(2008,1,1) && d.Date2 <= new DateTime(2008,2,1)
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
            result.Should().Contain(new { CurrencyCode = "USD", LastName = "Yang" });
        }

        [Test]
        public void FilterSeparationTest()
        {
            var query =
                from sales in _db.InternetSalesSet
                where 
                (sales.RelatedCurrency.CurrencyCode == "USD" || sales.RelatedCustomer.LastName == "Xu") &&
                (sales.RelatedCustomer.LastName == "Yang" ||  "Xu" == sales.RelatedCustomer.LastName ) && 
                sales.InternetTotalSales() > 0
                select new
                {
                    sales.RelatedCurrency.CurrencyCode,
                    sales.RelatedCustomer.LastName
                };
           
            var result = query.ToList();
            result.Should().Contain(new { CurrencyCode = "USD", LastName = "Yang" });
        }

    }
}
