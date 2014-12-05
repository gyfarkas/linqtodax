using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using AdventureWorks;
using FluentAssertions;
using LinqToDAX;
using LinqToDAX.Query;
using NUnit.Core;
using NUnit.Framework;
using System;
using System.Linq;
using TabularEntities;


namespace UnitTests
{
    [TestFixture]
    class LinqOperatorTests
    {
        private const string ConnectionString =
            "Provider=MSOLAP;Data Source=LDDEVCUBEDB2;Initial Catalog=AdventureWorks Tabular Model SQL 2012;";

        private static readonly AdventureWorksContext _db = new AdventureWorksContext(ConnectionString);

        [Test]
        public void SelectIntoTest()
        {
            var q =
                from g in _db.GeographySet
                from c in _db.CustomerSet
                select new
                {
                    g,
                    c
                }
                into cg
                select new
                {
                    cg.c.AddressLine1,
                    cg.g.City
                };
            var r = q.ToList();
            Assert.IsNotNull(r);
        }

        [Test]
        public void SelectManyTest()
        {
            var q =
                from c in _db.CustomerSet
                from s in _db.InternetSalesSet 
                select c.AddressLine1;
            var result = q.ToList();
            Assert.IsNotNull(result);
        }


        [Test]
        public void GroupByelementSelector()
        {

            var elems = new[]
            {
                new {s = "a", v = 1},
                new {s = "a", v = 1},
                new {s = "a", v = 1},
                new {s = "b", v = 1},
                new {s = "b", v = 1}

            };
            
            var res = elems.GroupBy(x => x.s, x => x.v).Select(g => System.Linq.Enumerable.Sum(g)).ToList();
            Assert.IsNotNull(res);

        }

        [Test]
        public void GroupByTest()
        {
            var list = new List<int>
            {
                1,2,3,4,5,6,7,8,9,10
            };
            var listq =
                from l in list
                group  l by new {i = l % 2}
                into g
                select new
                {
                    grp = g,
                    list = g.Select(x => x).First()
                };
            var r = listq.ToList();
            var f = r.First();
           var q =
                from gr in _db.GeographySet
                from c in _db.CustomerSet         
                group c by new
                {
                    c.Gender,
                    gr.City,
                    c.Education
                } into g
                select new
                {
                    g.Key.Gender,
                    Sum = g.Select(x => 2 * x.TotalCarsOwned + x.TotalChildren).Sum(),
                    Sum2 = g.Where(x => x.TotalChildren > 1).Select(x => x.TotalChildren).Sum(),
                    Sum3 = g.Select(x => new { x.YearlyIncome, x.TotalChildren, x.TotalCarsOwned })
                                .Where(x => x.TotalChildren > 1)
                                .Take(10)
                                .Sum(y => 2 * y.TotalCarsOwned)
                    //,Sum4 = g
                };

            var res = q.ToList();
            Assert.IsNotNull(res);
            //res.Where(x => x.Gender == "M").Select(x => x.Sum).Should().Contain(13346);

        }

        private void TestQuery<T>(IQueryable<T> exp)
        {
            var result = exp.ToList();
            Assert.IsNotNull(result);
        }

        [Test]
        public void WhereTest()
        {
            TestQuery(_db.CustomerSet.Where(c => c.RelatedGeography.City == "London"));
        }

        [Test]
        public void TestWhereTrue()
        {
            TestQuery(_db.CustomerSet.Where(c => true));
        }

        [Test]
        public void TestWhereFalse()
        {
            TestQuery(_db.CustomerSet.Where(c => false));
        }

        [Test]
        public void TestCompareConstructedEqual()
        {
            TestQuery(
                _db.CustomerSet.Select(x => new { x.RelatedGeography.City }).Where(c => new { x = c.City } == new { x = "London" })
                );
        }

        [Test]
        public void TestCompareConstructedMultiValueEqual()
        {
          
            var myentity = new {x = "London", y = "UK"};
            TestQuery(
                _db.CustomerSet.Select(x => new { x.RelatedGeography.City, x.RelatedGeography.CountryRegionName, level = "1"})
                .Where(c => new { x = c.City, y = c.CountryRegionName } == myentity)
                );
        }

        [Test]
        public void TestCompareConstructedMultiValueNotEqual()
        {
            TestQuery(
                _db.CustomerSet.Where(c => new { x = c.RelatedGeography.City, y = c.RelatedGeography.CountryRegionName } != new { x = "London", y = "UK" })
                );
        }

        [Test]
        public void TestCompareConstructed()
        {
            TestQuery(
                _db.CustomerSet.Where(c => new { x = c.RelatedGeography.City } == new { x = "London" })
                );
        }

        [Test]
        public void TestSelectScalar()
        {
            TestQuery(_db.CustomerSet.Select(c => c.RelatedGeography.City));
        }

        [Test]
        public void TestSelectAnonymousOne()
        {
            TestQuery(_db.CustomerSet.Select(c => new { c.RelatedGeography.City }));
        }

        [Test]
        public void TestSelectAnonymousTwo()
        {
            TestQuery(_db.CustomerSet.Select(c => new { c.RelatedGeography.City, c.Phone }));
        }

        [Test]
        public void TestSelectAnonymousThree()
        {
            TestQuery(_db.CustomerSet.Select(c => new { c.RelatedGeography.City, c.Phone, c.RelatedGeography.CountryRegionName }));
        }

        [Test]
        public void TestSelectCustomerTable()
        {
            TestQuery(_db.CustomerSet);
        }

        [Test]
        public void TestSelectCustomerIdentity()
        {
            TestQuery(_db.CustomerSet.Select(c => c));
        }

        [Test]
        public void TestSelectAnonymousWithObject()
        {
            TestQuery(_db.CustomerSet.Select(c => new { c.RelatedGeography.City, c }));
        }

        [Test]
        public void TestSelectAnonymousNested()
        {
            TestQuery(_db.CustomerSet.Select(c => new { c.RelatedGeography.City, Country = new { c.RelatedGeography.CountryRegionName } }));
        }

        [Test]
        public void TestSelectAnonymousEmpty()
        {
            TestQuery(_db.CustomerSet.Select(c => new { }));
        }

        [Test]
        public void TestSelectAnonymousLiteral()
        {
            TestQuery(_db.CustomerSet.Select(c => new { X = 10 }));
        }

        [Test]
        public void TestSelectConstantInt()
        {
            TestQuery(_db.CustomerSet.Select(c => 0));
        }

        [Test]
        public void TestSelectConstantNullString()
        {
            TestQuery(_db.CustomerSet.Select(c => (string)null));
        }

        [Test]
        public void TestSelectLocal()
        {
            int x = 10;
            TestQuery(_db.CustomerSet.Select(c => x));
        }

        [Test]
        public void TestSelectNestedCollection()
        {
            TestQuery(
                (from c in _db.CustomerSet
                where c.LastName == "Xu" 
                select _db.InternetSalesSet.Where(o => o.CustomerKey == c.CustomerKey && o.OrderDate.Year == 1997).Select(o => o.SalesAmount))
                );
        }

        [Test]
        public void TestSelectNestedCollectionInAnonymousType()
        {
            TestQuery(
                from c in _db.CustomerSet
                where c.LastName == "Xu"
                select new { Foos = _db.InternetSalesSet.Where(o => o.CustomerKey == c.CustomerKey && o.OrderDate.Year == 1997).Select(o => o.SalesAmount) }
                );
        }

        /*
         * 
         * 
         * 
        public void TestJoinCustomerOrders()
        {
            TestQuery(
                from c in db.Customers
                join o in db.Orders on c.CustomerID equals o.CustomerID
                select new { c.ContactName, o.OrderID }
                );
        }

        public void TestJoinMultiKey()
        {
            TestQuery(
                from c in db.Customers
                join o in db.Orders on new { a = c.CustomerID, b = c.CustomerID } equals new { a = o.CustomerID, b = o.CustomerID }
                select new { c, o }
                );
        }

        public void TestJoinIntoCustomersOrders()
        {
            TestQuery(
                from c in db.Customers
                join o in db.Orders on c.CustomerID equals o.CustomerID into ords
                select new { cust = c, ords = ords.ToList() }
                );
        }

        public void TestJoinIntoCustomersOrdersCount()
        {
            TestQuery(
                from c in db.Customers
                join o in db.Orders on c.CustomerID equals o.CustomerID into ords
                select new { cust = c, ords = ords.Count() }
                );
        }

        public void TestJoinIntoDefaultIfEmpty()
        {
            TestQuery(
                from c in db.Customers
                join o in db.Orders on c.CustomerID equals o.CustomerID into ords
                from o in ords.DefaultIfEmpty()
                select new { c, o }
                );
        }
         */
        [Test]
        public void TestSelectManyCustomerOrders()
        {
            TestQuery(
                from c in _db.CustomerSet
                from o in _db.InternetSalesSet
                where c.CustomerKey == o.CustomerKey
                select new { c.FirstName, o.OrderDate }
                );
        }

        [Test]
        public void TestMultipleJoinsWithJoinConditionsInWhere()
        {
            // this should reduce to inner joins
            TestQuery(
                from d in _db.SalesTerritorySet
                from c in _db.CustomerSet
                from o in _db.InternetSalesSet
                where o.CustomerKey == c.CustomerKey && o.SalesTerritoryKey == d.SalesTerritoryKey
                where c.CustomerId == "ALFKI"
                select d.SalesTerritoryCountry
                );
        }
        [Test]
        public void TestMultipleJoinsWithMissingJoinCondition()
        {
            // this should force a naked cross join
            TestQuery(
                from c in _db.CustomerSet
                from d in _db.SalesTerritorySet
                from o in _db.InternetSalesSet
                where o.CustomerKey == c.CustomerKey //&& o.SalesTerritoryKey == d.SalesTerritoryKey
                where c.CustomerId == "ALFKI"
                select d.SalesTerritoryCountry
                );
        }
        
        public void TestOrderBy()
        {
            TestQuery(
                _db.CustomerSet.OrderBy(c => c.TotalCarsOwned)
                );
        }

        public void TestOrderBySelect()
        {
            TestQuery(
                _db.CustomerSet.OrderBy(c => c.CustomerKey).Select(c => c.LastName)
                );
        }

        public void TestOrderByOrderBy()
        {
            TestQuery(
                _db.CustomerSet.OrderBy(c => c.CustomerId).OrderBy(c => c.LastName).Select(c => c.FirstName)
                );
        }

        public void TestOrderByThenBy()
        {
            TestQuery(
                _db.CustomerSet.OrderBy(c => c.CustomerId).ThenBy(c => c.LastName).Select(c => c.FirstName)
                );
        }

        public void TestOrderByDescending()
        {
            TestQuery(
                _db.CustomerSet.OrderByDescending(c => c.CustomerId).Select(c => c.LastName)
                );
        }

        public void TestOrderByDescendingThenBy()
        {
            TestQuery(
                _db.CustomerSet.OrderByDescending(c => c.CustomerId).ThenBy(c => c.LastName).Select(c => c.FirstName)
                );
        }

        public void TestOrderByDescendingThenByDescending()
        {
            TestQuery(
                _db.CustomerSet.OrderByDescending(c => c.LastName).ThenByDescending(c => c.FirstName).Select(c => c.AddressLine1)
                );
        }

        //public void TestOrderByJoin()
        //{
        //    TestQuery(
        //        from c in db.Customers.OrderBy(c => c.CustomerID)
        //        join o in db.Orders.OrderBy(o => o.OrderID) on c.CustomerID equals o.CustomerID
        //        select new { c.CustomerID, o.OrderID }
        //        );
        //}

        //public void TestOrderBySelectMany()
        //{
        //    TestQuery(
        //        from c in _db.CustomerSet.OrderBy(c => c.CustomerID)
        //        from o in _db.InternetSalesSet.OrderBy(o => o.OrderID)
        //        where c.CustomerID == o.CustomerID
        //        select new { c.ContactName, o.OrderID }
        //        );
        //}
        

        [Test]
        [ExpectedException(typeof(TabularException))]
        public void TestGroupBy()
        {
            TestQuery(
                _db.CustomerSet.GroupBy(c => c.RelatedGeography.City)
                );
        }

        [Test]
        public void TestGroupBySelectMany()
        {
            TestQuery(
                _db.CustomerSet
                .GroupBy(c => c.RelatedGeography.City)
                .SelectMany(g => g)
                );
        }

        [Test]
        public void TestGroupBySum()
        {
            TestQuery(
                _db.InternetSalesSet.GroupBy(o => o.CustomerKey).Select(g => g.Sum(o => o.SalesAmount))
                );
        }

        [Test]
        public void TestGroupByCount()
        {
            TestQuery(
                 _db.InternetSalesSet.GroupBy(o => o.CustomerKey).Select(g => g.Select(x => x.ShipDate).DistinctCount())
                );
        }

        [Test]
        public void TestGroupByLongCount()
        {
            TestQuery(
                 _db.InternetSalesSet.GroupBy(o => o.CustomerKey).Select(g => g.LongCount())
                );
        }

        [Test]
        public void TestGroupBySumMinMaxAvg()
        {
            TestQuery(
                 _db.InternetSalesSet.GroupBy(o => o.CustomerKey).Select(g =>
                    new
                    {
                        Sum = g.Sum(o => o.CustomerKey),
                        Min = g.Min(o => o.CustomerKey),
                        Max = g.Max(o => o.CustomerKey),
                        Avg = g.Average(o => o.CustomerKey)
                    })
                );
        }

        [Test]
        public void TestGroupByWithResultSelector()
        {
           
            TestQuery(
                _db.InternetSalesSet.GroupBy(o => o.CustomerKey, (k, g) =>
                    new
                    {
                        Sum = g.Sum(o => o.OrderQuantity),
                        Min = g.Min(o => o.OrderQuantity),
                        Max = g.Max(o => o.OrderQuantity),
                        Avg = g.Average(o => o.OrderQuantity)
                    })
                );
        }

        [Test]
        public void TestGroupByWithElementSelectorSum()
        {
            var s = _db.InternetSalesSet.GroupBy(o => o.CustomerKey, o => o.SalesAmount).Select(g => g.Sum());
            TestQuery(
                _db.InternetSalesSet.GroupBy(o => o.CustomerKey, o => o.SalesAmount).Select(g => g.Sum())
                );
        }

        [Test]
        [ExpectedException(typeof(TabularException))]
        public void TestGroupByWithElementSelector()
        {
            // note: groups are retrieved through a separately execute subquery per row
            var r = _db.InternetSalesSet.GroupBy(o => o.CustomerKey, o => o.SalesOrderNumber).Select(x => x);
            TestQuery(
                _db.InternetSalesSet.GroupBy(o => o.CustomerKey, o => o.SalesOrderNumber)
                );
        }

        [Test]
        public void TestGroupByWithElementSelectorSumMax()
        {
            TestQuery(
                _db.InternetSalesSet.GroupBy(o => o.CustomerKey, o => o.OrderQuantity).Select(g => new { Sum = g.Sum(), Max = g.Max() })
                );
        }

        [Test]
        public void TestGroupByWithAnonymousElement()
        {
            TestQuery(
                 _db.InternetSalesSet
                 .GroupBy(o => o.CustomerKey, o => new { o.SalesTerritoryKey })
                 .Select(g => g.Sum(x => x.SalesTerritoryKey))
                );
        }

        [Test]
        public void TestGroupByWithTwoPartKey()
        {
            TestQuery(
                _db.InternetSalesSet.GroupBy(o => new { o.CustomerKey, o.OrderDate }).Select(g => g.Sum(o => o.SalesAmount))
                );
        }

        //[Test]
        //public void TestSumWithNoArg()
        //{
        //    TestQuery(
        //        () => _db.InternetSalesSet.Select(o => o.SalesAmount).Sum()
        //        );
        //}

        //[Test]
        //public void TestSumWithArg()
        //{
        //    TestQuery(
        //        () => _db.InternetSalesSet.Sum(o => o.SalesAmount)
        //        );
        //}
        
        //[Test]
        //public void TestCountWithNoPredicate()
        //{
        //    TestQuery(
        //        () => _db.InternetSalesSet.Count()
        //        );
        //}

        //[Test]
        //public void TestCountWithPredicate()
        //{
        //    TestQuery(
        //        () => _db.InternetSalesSet.Count(o => o.RelatedCustomer.LastName == "ALFKI")
        //        );
        //}

        [Test]
        public void TestDistinct()
        {
            TestQuery(
                _db.CustomerSet.Distinct()
                );
        }

        [Test]
        public void TestDistinctScalar()
        {
            TestQuery(
                _db.CustomerSet.Select(c => c.RelatedGeography.City).Distinct()
                );
        }

        [Test]
        public void TestOrderByDistinct()
        {
            TestQuery(
                _db.CustomerSet.OrderBy(c => c.CustomerKey).Select(c => new {c.RelatedGeography.City }).Distinct()
                );
        }

        [Test]
        public void TestDistinctOrderBy()
        {
            TestQuery(
                _db.CustomerSet.Select(c => c.RelatedGeography.City).Distinct().OrderBy(c => c)
                );
        }

        //[Test]
        //public void TestDistinctCount()
        //{
        //    TestQuery(
        //        () => _db.CustomerSet.Distinct().Count()
        //        );
        //}

        //[Test]
        //public void TestSelectDistinctCount()
        //{
        //    // cannot do: SELECT COUNT(DISTINCT some-colum) FROM some-table
        //    // because COUNT(DISTINCT some-column) does not count nulls
        //    TestQuery(
        //        () => _db.CustomerSet.Select(c => c.RelatedGeography.City).Distinct().Count()
        //        );
        //}

        //[Test]
        //public void TestSelectSelectDistinctCount()
        //{
        //    TestQuery(
        //        () => db.Customers.Select(c => c.City).Select(c => c).Distinct().Count()
        //        );
        //}

        // [Test]
        //public void TestDistinctCountPredicate()
        //{
        //    TestQuery(
        //        () => db.Customers.Distinct().Count(c => c.CustomerID == "ALFKI")
        //        );
        //}

        //[Test]
        //public void TestDistinctSumWithArg()
        //{
        //    TestQuery(
        //        () => db.Orders.Distinct().Sum(o => o.OrderID)
        //        );
        //}

        //[Test]
        //public void TestSelectDistinctSum()
        //{
        //    TestQuery(
        //        () => db.Orders.Select(o => o.OrderID).Distinct().Sum()
        //        );
        //}
        

        [Test]
        public void TestTake()
        {
            TestQuery(
                _db.InternetSalesSet.Take(5)
                );
        }

        
        public void TestTakeDistinct()
        {
            // distinct must be forced to apply after top has been computed
            TestQuery(
                 _db.InternetSalesSet.Take(5).Distinct()
                );
        }

        public void TestDistinctTake()
        {
            // top must be forced to apply after distinct has been computed
            TestQuery(
                 _db.InternetSalesSet.Distinct().Take(5)
                );
        }

        //public void TestDistinctTakeCount()
        //{
        //    TestQuery(
        //        () =>  _db.InternetSalesSet.Distinct().Take(5).Count()
        //        );
        //}

        //public void TestTakeDistinctCount()
        //{
        //    TestQuery(
        //        () => db.Orders.Take(5).Distinct().Count()
        //        );
        //}

        //[ExcludeProvider("Access")]
        //[ExcludeProvider("SqlCe")]
        //public void TestSkip()
        //{
        //    TestQuery(
        //        db.Customers.OrderBy(c => c.ContactName).Skip(5)
        //        );
        //}

        //[ExcludeProvider("Access")]
        //[ExcludeProvider("SqlCe")]
        //public void TestTakeSkip()
        //{
        //    TestQuery(
        //        db.Customers.OrderBy(c => c.ContactName).Take(10).Skip(5)
        //        );
        //}

        //[ExcludeProvider("Access")]
        //[ExcludeProvider("SqlCe")]
        //public void TestDistinctSkip()
        //{
        //    TestQuery(
        //        db.Customers.Distinct().OrderBy(c => c.ContactName).Skip(5)
        //        );
        //}
        /*
        public void TestSkipTake()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Skip(5).Take(10)
                );
        }

        public void TestDistinctSkipTake()
        {
            TestQuery(
                db.Customers.Distinct().OrderBy(c => c.ContactName).Skip(5).Take(10)
                );
        }

        [ExcludeProvider("Access")]
        [ExcludeProvider("SqlCe")]
        public void TestSkipDistinct()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Skip(5).Distinct()
                );
        }

        //[ExcludeProvider("Access")]
        public void TestSkipTakeDistinct()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Skip(5).Take(10).Distinct()
                );
        }

        [ExcludeProvider("Access")]
        [ExcludeProvider("SqlCe")]
        public void TestTakeSkipDistinct()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Take(10).Skip(5).Distinct()
                );
        }

        public void TestFirst()
        {
            TestQuery(
                () => db.Customers.OrderBy(c => c.ContactName).First()
                );
        }

        public void TestFirstPredicate()
        {
            TestQuery(
                () => db.Customers.OrderBy(c => c.ContactName).First(c => c.City == "London")
                );
        }

        public void TestWhereFirst()
        {
            TestQuery(
                () => db.Customers.OrderBy(c => c.ContactName).Where(c => c.City == "London").First()
                );
        }

        public void TestFirstOrDefault()
        {
            TestQuery(
                () => db.Customers.OrderBy(c => c.ContactName).FirstOrDefault()
                );
        }

        public void TestFirstOrDefaultPredicate()
        {
            TestQuery(
                () => db.Customers.OrderBy(c => c.ContactName).FirstOrDefault(c => c.City == "London")
                );
        }

        public void TestWhereFirstOrDefault()
        {
            TestQuery(
                () => db.Customers.OrderBy(c => c.ContactName).Where(c => c.City == "London").FirstOrDefault()
                );
        }

        public void TestReverse()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Reverse()
                );
        }

        public void TestReverseReverse()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Reverse().Reverse()
                );
        }

        public void TestReverseWhereReverse()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Reverse().Where(c => c.City == "London").Reverse()
                );
        }

        public void TestReverseTakeReverse()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Reverse().Take(5).Reverse()
                );
        }

        public void TestReverseWhereTakeReverse()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Reverse().Where(c => c.City == "London").Take(5).Reverse()
                );
        }

        public void TestLast()
        {
            TestQuery(
                () => db.Customers.OrderBy(c => c.ContactName).Last()
                );
        }

        public void TestLastPredicate()
        {
            TestQuery(
                () => db.Customers.OrderBy(c => c.ContactName).Last(c => c.City == "London")
                );
        }

        public void TestWhereLast()
        {
            TestQuery(
                () => db.Customers.OrderBy(c => c.ContactName).Where(c => c.City == "London").Last()
                );
        }

        public void TestLastOrDefault()
        {
            TestQuery(
                () => db.Customers.OrderBy(c => c.ContactName).LastOrDefault()
                );
        }

        public void TestLastOrDefaultPredicate()
        {
            TestQuery(
                () => db.Customers.OrderBy(c => c.ContactName).LastOrDefault(c => c.City == "London")
                );
        }

        public void TestWhereLastOrDefault()
        {
            TestQuery(
                () => db.Customers.OrderBy(c => c.ContactName).Where(c => c.City == "London").LastOrDefault()
                );
        }

        public void TestSingle()
        {
            TestQueryFails(
                () => db.Customers.Single()
                );
        }

        public void TestSinglePredicate()
        {
            TestQuery(
                () => db.Customers.Single(c => c.CustomerID == "ALFKI")
                );
        }

        public void TestWhereSingle()
        {
            TestQuery(
                () => db.Customers.Where(c => c.CustomerID == "ALFKI").Single()
                );
        }

        public void TestSingleOrDefault()
        {
            TestQueryFails(
                () => db.Customers.SingleOrDefault()
                );
        }

        public void TestSingleOrDefaultPredicate()
        {
            TestQuery(
                () => db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI")
                );
        }

        public void TestWhereSingleOrDefault()
        {
            TestQuery(
                () => db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault()
                );
        }

        public void TestAnyWithSubquery()
        {
            TestQuery(
                db.Customers.Where(c => db.Orders.Where(o => o.CustomerID == c.CustomerID).Any(o => o.OrderDate.Year == 1997))
                );
        }

        public void TestAnyWithSubqueryNoPredicate()
        {
            TestQuery(
                db.Customers.Where(c => db.Orders.Where(o => o.CustomerID == c.CustomerID).Any())
                );
        }

        public void TestAnyWithLocalCollection()
        {
            string[] ids = new[] { "ABCDE", "ALFKI" };
            TestQuery(
                db.Customers.Where(c => ids.Any(id => c.CustomerID == id))
                );
        }

        public void TestAnyTopLevel()
        {
            TestQuery(
                () => db.Customers.Any()
                );
        }

        public void TestAllWithSubquery()
        {
            TestQuery(
                db.Customers.Where(c => db.Orders.Where(o => o.CustomerID == c.CustomerID).All(o => o.OrderDate.Year == 1997))
                );
        }

        public void TestAllWithLocalCollection()
        {
            string[] patterns = new[] { "a", "e" };

            TestQuery(
                db.Customers.Where(c => patterns.All(p => c.ContactName.Contains(p)))
                );
        }

        public void TestAllTopLevel()
        {
            TestQuery(
                () => db.Customers.All(c => c.ContactName.StartsWith("a"))
                );
        }

        public void TestContainsWithSubquery()
        {
            TestQuery(
                db.Customers.Where(c => db.Orders.Select(o => o.CustomerID).Contains(c.CustomerID))
                );
        }

        public void TestContainsWithLocalCollection()
        {
            string[] ids = new[] { "ABCDE", "ALFKI" };
            TestQuery(
                db.Customers.Where(c => ids.Contains(c.CustomerID))
                );
        }

        public void TestContainsTopLevel()
        {
            TestQuery(
                () => db.Customers.Select(c => c.CustomerID).Contains("ALFKI")
                );
        }



        public void TestCoalesce()
        {
            TestQuery(db.Customers.Where(c => (c.City ?? "Seattle") == "Seattle"));
        }

        public void TestCoalesce2()
        {
            TestQuery(db.Customers.Where(c => (c.City ?? c.Country ?? "Seattle") == "Seattle"));
        }


        // framework function tests

        public void TestStringLength()
        {
            TestQuery(db.Customers.Where(c => c.City.Length == 7));
        }

        public void TestStringStartsWithLiteral()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.StartsWith("M")));
        }

        public void TestStringStartsWithColumn()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.StartsWith(c.ContactName)));
        }

        public void TestStringEndsWithLiteral()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.EndsWith("s")));
        }

        public void TestStringEndsWithColumn()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.EndsWith(c.ContactName)));
        }

        public void TestStringContainsLiteral()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.Contains("and")));
        }

        public void TestStringContainsColumn()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.Contains(c.ContactName)));
        }

        public void TestStringConcatImplicit2Args()
        {
            TestQuery(db.Customers.Where(c => c.ContactName + "X" == "X"));
        }

        public void TestStringConcatExplicit2Args()
        {
            TestQuery(db.Customers.Where(c => string.Concat(c.ContactName, "X") == "X"));
        }

        public void TestStringConcatExplicit3Args()
        {
            TestQuery(db.Customers.Where(c => string.Concat(c.ContactName, "X", c.Country) == "X"));
        }

        public void TestStringConcatExplicitNArgs()
        {
            TestQuery(db.Customers.Where(c => string.Concat(new string[] { c.ContactName, "X", c.Country }) == "X"));
        }

        public void TestStringIsNullOrEmpty()
        {
            TestQuery(db.Customers.Where(c => string.IsNullOrEmpty(c.City)));
        }

        public void TestStringToUpper()
        {
            TestQuery(db.Customers.Where(c => c.City.ToUpper() == "SEATTLE"));
        }

        public void TestStringToLower()
        {
            TestQuery(db.Customers.Where(c => c.City.ToLower() == "seattle"));
        }

        public void TestStringSubstring()
        {
            TestQuery(db.Customers.Where(c => c.City.Substring(0, 4) == "Seat"));
        }

        public void TestStringSubstringNoLength()
        {
            TestQuery(db.Customers.Where(c => c.City.Substring(4) == "tle"));
        }

        [ExcludeProvider("SQLite")]
        public void TestStringIndexOf()
        {
            TestQuery(db.Customers.Where(c => c.City.IndexOf("tt") == 4));
        }

        [ExcludeProvider("SQLite")]
        public void TestStringIndexOfChar()
        {
            TestQuery(db.Customers.Where(c => c.City.IndexOf('t') == 4));
        }

        public void TestStringTrim()
        {
            TestQuery(db.Customers.Where(c => c.City.Trim() == "Seattle"));
        }

        public void TestStringToString()
        {
            // just to prove this is a no op
            TestQuery(db.Customers.Where(c => c.City.ToString() == "Seattle"));
        }

        [ExcludeProvider("Access")]
        public void TestStringReplace()
        {
            TestQuery(db.Customers.Where(c => c.City.Replace("ea", "ae") == "Saettle"));
        }

        [ExcludeProvider("Access")]
        public void TestStringReplaceChars()
        {
            TestQuery(db.Customers.Where(c => c.City.Replace("e", "y") == "Syattly"));
        }

        [ExcludeProvider("Access")]
        public void TestStringRemove()
        {
            TestQuery(db.Customers.Where(c => c.City.Remove(1, 2) == "Sttle"));
        }

        [ExcludeProvider("Access")]
        public void TestStringRemoveNoCount()
        {
            TestQuery(db.Customers.Where(c => c.City.Remove(4) == "Seat"));
        }

        public void TestDateTimeConstructYMD()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate == new DateTime(o.OrderDate.Year, 1, 1)));
        }

        public void TestDateTimeConstructYMDHMS()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate == new DateTime(o.OrderDate.Year, 1, 1, 10, 25, 55)));
        }

        public void TestDateTimeDay()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Day == 5));
        }

        public void TestDateTimeMonth()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Month == 12));
        }

        public void TestDateTimeYear()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Year == 1997));
        }

        public void TestDateTimeHour()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Hour == 6));
        }

        public void TestDateTimeMinute()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Minute == 32));
        }

        public void TestDateTimeSecond()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Second == 47));
        }

        [ExcludeProvider("Access")]
        public void TestDateTimeMillisecond()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Millisecond == 200));
        }

        [ExcludeProvider("Access")]
        public void TestDateTimeDayOfYear()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.DayOfYear == 360));
        }

        public void TestDateTimeDayOfWeek()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.DayOfWeek == DayOfWeek.Friday));
        }

        public void TestMathAbs()
        {
            TestQuery(db.Orders.Where(o => Math.Abs(o.OrderID) == 10));
        }

        public void TestMathAtan()
        {
            TestQuery(db.Orders.Where(o => Math.Atan(o.OrderID) == 0));
        }

        public void TestMathCos()
        {
            TestQuery(db.Orders.Where(o => Math.Cos(o.OrderID) == 0));
        }

        public void TestMathSin()
        {
            TestQuery(db.Orders.Where(o => Math.Sin(o.OrderID) == 0));
        }

        public void TestMathTan()
        {
            TestQuery(db.Orders.Where(o => Math.Tan(o.OrderID) == 0));
        }

        public void TestMathExp()
        {
            TestQuery(db.Orders.Where(o => Math.Exp(o.OrderID < 1000 ? 1 : 2) == 0));
        }

        public void TestMathLog()
        {
            TestQuery(db.Orders.Where(o => Math.Log(o.OrderID) == 0));
        }

        public void TestMathSqrt()
        {
            TestQuery(db.Orders.Where(o => Math.Sqrt(o.OrderID) == 0));
        }

        public void TestMathPow()
        {
            TestQuery(db.Orders.Where(o => Math.Pow(o.OrderID < 1000 ? 1 : 2, 3) == 0));
        }

        public void TestMathRoundDefault()
        {
            TestQuery(db.Orders.Where(o => Math.Round((decimal)o.OrderID) == 0));
        }

        [ExcludeProvider("Access")]
        [ExcludeProvider("SqlCe")]
        public void TestMathAcos()
        {
            TestQuery(db.Orders.Where(o => Math.Acos(1.0/o.OrderID) == 0));
        }

        [ExcludeProvider("Access")]
        [ExcludeProvider("SqlCe")]
        public void TestMathAsin()
        {
            TestQuery(db.Orders.Where(o => Math.Asin(1.0/o.OrderID) == 0));
        }

        [ExcludeProvider("Access")]
        public void TestMathAtan2()
        {
            TestQuery(db.Orders.Where(o => Math.Atan2(1.0/o.OrderID, 3) == 0));
        }

        [ExcludeProvider("SQLite")]
        [ExcludeProvider("Access")]
        public void TestMathLog10()
        {
            TestQuery(db.Orders.Where(o => Math.Log10(o.OrderID) == 0));
        }

        [ExcludeProvider("SQLite")]
        [ExcludeProvider("Access")]
        public void TestMathCeiling()
        {
            TestQuery(db.Orders.Where(o => Math.Ceiling((double)o.OrderID) == 0));
        }

        [ExcludeProvider("Access")]
        public void TestMathRoundToPlace()
        {
            TestQuery(db.Orders.Where(o => Math.Round((decimal)o.OrderID, 2) == 0));
        }

        [ExcludeProvider("SQLite")]
        [ExcludeProvider("Access")]
        public void TestMathFloor()
        {
            TestQuery(db.Orders.Where(o => Math.Floor((double)o.OrderID) == 0));
        }

        [ExcludeProvider("SQLite")]
        public void TestMathTruncate()
        {
            TestQuery(db.Orders.Where(o => Math.Truncate((double)o.OrderID) == 0));
        }

        public void TestStringCompareToLT()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") < 0));
        }

        public void TestStringCompareToLE()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") <= 0));
        }

        public void TestStringCompareToGT()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") > 0));
        }

        public void TestStringCompareToGE()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") >= 0));
        }

        public void TestStringCompareToEQ()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") == 0));
        }

        public void TestStringCompareToNE()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") != 0));
        }

        public void TestStringCompareLT()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") < 0));
        }

        public void TestStringCompareLE()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") <= 0));
        }

        public void TestStringCompareGT()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") > 0));
        }

        public void TestStringCompareGE()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") >= 0));
        }

        public void TestStringCompareEQ()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") == 0));
        }

        public void TestStringCompareNE()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") != 0));
        }

        public void TestIntCompareTo()
        {
            // prove that x.CompareTo(y) works for types other than string
            TestQuery(db.Orders.Where(o => o.OrderID.CompareTo(1000) == 0));
        }

        public void TestDecimalCompare()
        {
            // prove that type.Compare(x,y) works with decimal
            TestQuery(db.Orders.Where(o => decimal.Compare((decimal)o.OrderID, 0.0m) == 0));
        }

        public void TestDecimalAdd()
        {
            TestQuery(db.Orders.Where(o => decimal.Add(o.OrderID, 0.0m) == 0.0m));
        }

        public void TestDecimalSubtract()
        {
            TestQuery(db.Orders.Where(o => decimal.Subtract(o.OrderID, 0.0m) == 0.0m));
        }

        public void TestDecimalMultiply()
        {
            TestQuery(db.Orders.Where(o => decimal.Multiply(o.OrderID, 1.0m) == 1.0m));
        }

        public void TestDecimalDivide()
        {
            TestQuery(db.Orders.Where(o => decimal.Divide(o.OrderID, 1.0m) == 1.0m));
        }

        [ExcludeProvider("SqlCe")]
        public void TestDecimalRemainder()
        {
            TestQuery(db.Orders.Where(o => decimal.Remainder(o.OrderID, 1.0m) == 0.0m));
        }

        public void TestDecimalNegate()
        {
            TestQuery(db.Orders.Where(o => decimal.Negate(o.OrderID) == 1.0m));
        }

        public void TestDecimalRoundDefault()
        {
            TestQuery(db.Orders.Where(o => decimal.Round(o.OrderID) == 0m));
        }

        [ExcludeProvider("Access")]
        public void TestDecimalRoundPlaces()
        {
            TestQuery(db.Orders.Where(o => decimal.Round(o.OrderID, 2) == 0.00m));
        }

        [ExcludeProvider("SQLite")]
        public void TestDecimalTruncate()
        {
            TestQuery(db.Orders.Where(o => decimal.Truncate(o.OrderID) == 0m));
        }

        [ExcludeProvider("SQLite")]
        [ExcludeProvider("Access")]
        public void TestDecimalCeiling()
        {
            TestQuery(db.Orders.Where(o => decimal.Ceiling(o.OrderID) == 0.0m));
        }

        [ExcludeProvider("SQLite")]
        [ExcludeProvider("Access")]
        public void TestDecimalFloor()
        {
            TestQuery(db.Orders.Where(o => decimal.Floor(o.OrderID) == 0.0m));
        }

        public void TestDecimalLT()
        {
            // prove that decimals are treated normally with respect to normal comparison operators
            TestQuery(db.Orders.Where(o => ((decimal)o.OrderID) < 0.0m));
        }

        public void TestIntLessThan()
        {
            TestQuery(db.Orders.Where(o => o.OrderID < 0));
        }

        public void TestIntLessThanOrEqual()
        {
            TestQuery(db.Orders.Where(o => o.OrderID <= 0));
        }

        public void TestIntGreaterThan()
        {
            TestQuery(db.Orders.Where(o => o.OrderID > 0));
        }

        public void TestIntGreaterThanOrEqual()
        {
            TestQuery(db.Orders.Where(o => o.OrderID >= 0));
        }

        public void TestIntEqual()
        {
            TestQuery(db.Orders.Where(o => o.OrderID == 0));
        }

        public void TestIntNotEqual()
        {
            TestQuery(db.Orders.Where(o => o.OrderID != 0));
        }

        public void TestIntAdd()
        {
            TestQuery(db.Orders.Where(o => o.OrderID + 0 == 0));
        }

        public void TestIntSubtract()
        {
            TestQuery(db.Orders.Where(o => o.OrderID - 0 == 0));
        }

        public void TestIntMultiply()
        {
            TestQuery(db.Orders.Where(o => o.OrderID * 1 == 1));
        }

        public void TestIntDivide()
        {
            TestQuery(db.Orders.Where(o => o.OrderID / 1 == 1));
        }

        public void TestIntModulo()
        {
            TestQuery(db.Orders.Where(o => o.OrderID % 1 == 0));
        }

        public void TestIntLeftShift()
        {
            TestQuery(db.Orders.Where(o => o.OrderID << 1 == 0));
        }

        public void TestIntRightShift()
        {
            TestQuery(db.Orders.Where(o => o.OrderID >> 1 == 0));
        }

        public void TestIntBitwiseAnd()
        {
            TestQuery(db.Orders.Where(o => (o.OrderID & 1) == 0));
        }

        public void TestIntBitwiseOr()
        {
            TestQuery(db.Orders.Where(o => (o.OrderID | 1) == 1));
        }

        public void TestIntBitwiseExclusiveOr()
        {
            TestQuery(db.Orders.Where(o => (o.OrderID ^ 1) == 1));
        }

        public void TestIntBitwiseNot()
        {
            TestQuery(db.Orders.Where(o => ~o.OrderID == 0));
        }

        public void TestIntNegate()
        {
            TestQuery(db.Orders.Where(o => -o.OrderID == -1));
        }

        public void TestAnd()
        {
            TestQuery(db.Orders.Where(o => o.OrderID > 0 && o.OrderID < 2000));
        }

        public void TestOr()
        {
            TestQuery(db.Orders.Where(o => o.OrderID < 5 || o.OrderID > 10));
        }

        public void TestNot()
        {
            TestQuery(db.Orders.Where(o => !(o.OrderID == 0)));
        }

        public void TestEqualNull()
        {
            TestQuery(db.Customers.Where(c => c.City == null));
        }

        public void TestEqualNullReverse()
        {
            TestQuery(db.Customers.Where(c => null == c.City));
        }

        public void TestConditional()
        {
            TestQuery(db.Orders.Where(o => (o.CustomerID == "ALFKI" ? 1000 : 0) == 1000));
        }

        public void TestConditional2()
        {
            TestQuery(db.Orders.Where(o => (o.CustomerID == "ALFKI" ? 1000 : o.CustomerID == "ABCDE" ? 2000 : 0) == 1000));
        }

        public void TestConditionalTestIsValue()
        {
            TestQuery(db.Orders.Where(o => (((bool)(object)o.OrderID) ? 100 : 200) == 100));
        }

        public void TestConditionalResultsArePredicates()
        {
            TestQuery(db.Orders.Where(o => (o.CustomerID == "ALFKI" ? o.OrderID < 10 : o.OrderID > 10)));
        }

        public void TestSelectManyJoined()
        {
            TestQuery(
                from c in db.Customers
                from o in db.Orders.Where(o => o.CustomerID == c.CustomerID)
                select new { c.ContactName, o.OrderDate }
                );
        }

        public void TestSelectManyJoinedDefaultIfEmpty()
        {
            TestQuery(
                from c in db.Customers
                from o in db.Orders.Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                select new { c.ContactName, o.OrderDate }
                );
        }

        public void TestSelectWhereAssociation()
        {
            TestQuery(
                from o in db.Orders
                where o.Customer.City == "Seattle"
                select o
                );
        }

        public void TestSelectWhereAssociations()
        {
            TestQuery(
                from o in db.Orders
                where o.Customer.City == "Seattle" && o.Customer.Phone != "555 555 5555"
                select o
                );
        }

        public void TestSelectWhereAssociationTwice()
        {
            TestQuery(
                from o in db.Orders
                where o.Customer.City == "Seattle" && o.Customer.Phone != "555 555 5555"
                select o
                );
        }

        public void TestSelectAssociation()
        {
            TestQuery(
                from o in db.Orders
                select o.Customer
                );
        }

        public void TestSelectAssociations()
        {
            TestQuery(
                from o in db.Orders
                select new { A = o.Customer, B = o.Customer }
                );
        }

        public void TestSelectAssociationsWhereAssociations()
        {
            TestQuery(
                from o in db.Orders
                where o.Customer.City == "Seattle"
                where o.Customer.Phone != "555 555 5555"
                select new { A = o.Customer, B = o.Customer }
                );
        }

        public void TestCustomersIncludeOrders()
        {
            var policy = new EntityPolicy();
            policy.IncludeWith<Customer>(c => c.Orders);
            Northwind nw = new Northwind(this.provider.New(policy));

            TestQuery(
                nw.Customers
                );
        }

        public void TestCustomersIncludeOrdersDeferred()
        {
            var policy = new EntityPolicy();
            policy.IncludeWith<Customer>(c => c.Orders, true);
            Northwind nw = new Northwind(this.provider.New(policy));

            TestQuery(
                nw.Customers
                );
        }

        public void TestCustomersIncludeOrdersViaConstructorOnly()
        {
            var mapping = new AttributeMapping(typeof(NorthwindX));
            var policy = new EntityPolicy();
            policy.IncludeWith<CustomerX>(c => c.Orders);
            NorthwindX nw = new NorthwindX(this.provider.New(policy).New(mapping));

            TestQuery(
                nw.Customers
                );
        }

        public void TestCustomersWhereIncludeOrders()
        {
            var policy = new EntityPolicy();
            policy.IncludeWith<Customer>(c => c.Orders);
            Northwind nw = new Northwind(this.provider.New(policy));

            TestQuery(
                from c in nw.Customers
                where c.City == "London"
                select c
                );
        }

        public void TestCustomersIncludeOrdersAndDetails()
        {
            var policy = new EntityPolicy();
            policy.IncludeWith<Customer>(c => c.Orders);
            policy.IncludeWith<Order>(o => o.Details);
            Northwind nw = new Northwind(this.provider.New(policy));

            TestQuery(
                nw.Customers
                );
        }

        public void TestCustomersWhereIncludeOrdersAndDetails()
        {
            var policy = new EntityPolicy();
            policy.IncludeWith<Customer>(c => c.Orders);
            policy.IncludeWith<Order>(o => o.Details);
            Northwind nw = new Northwind(this.provider.New(policy));

            TestQuery(
                from c in nw.Customers
                where c.City == "London"
                select c
                );
        }

        public void TestInterfaceElementTypeAsGenericConstraint()
        {
            TestQuery(
                GetById(db.Products, 5)
                );
        }

        private static IQueryable<T> GetById<T>(IQueryable<T> query, int id) where T : IEntity
        {
            return query.Where(x => x.ID == id);
        }

        public void TestXmlMappingSelectCustomers()
        {
            var nw = new Northwind(this.provider.New(XmlMapping.FromXml(File.ReadAllText(@"Northwind.xml"))));

            TestQuery(
                from c in db.Customers
                where c.City == "London"
                select c.ContactName
                );
        }

        public void TestSingletonAssociationWithMemberAccess()
        {
            TestQuery(
                from o in db.Orders
                where o.Customer.City == "Seattle"
                where o.Customer.Phone != "555 555 5555"
                select new { A = o.Customer, B = o.Customer.City }
                );
        }

        public void TestCompareDateTimesWithDifferentNullability()
        {
            TestQuery(
                from o in db.Orders
                where o.OrderDate < DateTime.Today && ((DateTime?)o.OrderDate) < DateTime.Today
                select o
                );
        }

        public void TestContainsWithEmptyLocalList()
        {
            var ids = new string[0];
            TestQuery(
                from c in db.Customers
                where ids.Contains(c.CustomerID)
                select c
                );
        }

        public void TestContainsWithSubQuery()
        {
            var custsInLondon = db.Customers.Where(c => c.City == "London").Select(c => c.CustomerID);

            TestQuery(
                from c in db.Customers
                where custsInLondon.Contains(c.CustomerID)
                select c
                );
        }

        public void TestCombineQueriesDeepNesting()
        {
            var custs = db.Customers.Where(c => c.ContactName.StartsWith("xxx"));
            var ords = db.Orders.Where(o => custs.Any(c => c.CustomerID == o.CustomerID));
            TestQuery(
                db.OrderDetails.Where(d => ords.Any(o => o.OrderID == d.OrderID))
                );
        }

        public void TestLetWithSubquery()
        {
            TestQuery(
                from customer in db.Customers
                let orders =
                    from order in db.Orders
                    where order.CustomerID == customer.CustomerID
                    select order
                select new
                {
                    Customer = customer,
                    OrdersCount = orders.Count(),
                }
                );
        }
    }
         */
    }
}
