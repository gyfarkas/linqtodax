using System.Collections.Generic;
using System.Runtime.InteropServices;
using AdventureWorks;
using FluentAssertions;
using LinqToDAX.Query;
using LinqToDAX.Query.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using System;
using System.Linq;
using TabularEntities;
using AdventureWorks.Fakes;

namespace UnitTests
{
    public class Something
    {
        public string Cat { get; set; }
        public decimal? sum { get; set; }
    }

    public class Other
    {
        public List<string> List { get; set; }
        public decimal? Sum { get; set; }
        public bool Test { get; set; }
    }
    [TestClass]
    public class MockedTests
    {
        [TestMethod]
        public void Test()
        {
            using (ShimsContext.Create())
            {
                ShimTabularQueryExtensions.CalculateTableOf2IQueryableOfM0IQueryableOfM1<string,string>( (x,y) => x );
                ShimTabularQueryExtensions.GenerateOf3IQueryableOfM1IQueryableOfM2ExpressionOfFuncOfM1M2M0<Other,string,Something>((x,y,z) => new EnumerableQuery<Other>(new Other[]
                {
                    new Other { Sum = 10, List = new List<string> { "a", "b" ,"c"}}
                }));
                var context = new StubIAdventureworks();
                context.CurrencySetGet = () => new List<Currency>().AsQueryable();
                context.ProductCategorySetGet = () => new EnumerableQuery<ProductCategory>(new List<ProductCategory>());
                context.ResellerSalesSetGet = () => new EnumerableQuery<ResellerSales>(new ResellerSales[]{ });
                context.SalesTerritorySetGet = () => new EnumerableQuery<SalesTerritory>(new SalesTerritory[]{});
                this.QueryTest(context);
            }
        }

        private void QueryTest(IAdventureworks _db)
        {
            var q =
                from c in _db.SalesTerritorySet
                from d in _db.CustomerSet
                where d.NameStyle == true && d.CustomerKey.ApplyRelationship("FDFa")
                select c.SalesTerritoryGroup;
            var filter =
                from c in _db.SalesTerritorySet
                from d in _db.CustomerSet
                where c.SalesTerritoryCountry == "USA" &&  d.NameStyle == true && d.CustomerKey.ApplyRelationship("FDFa")
                select c.SalesTerritoryGroup;
            var q2 =
                from sales in _db.ProductCategorySet
                from d in _db.CustomerSet
                where d.NameStyle == true && d.CustomerKey.ApplyRelationship("FDFa")
                select new Something
                {
                    Cat = sales.ProductCategoryName,
                    sum = (from s in _db.ResellerSalesSet select new { s.SalesAmount }).Sumx(x => x.SalesAmount)
                };
            var result = q.CalculateTable(filter).Generate(q2, (x, y) => new Other { List = new List<string>{ x , y.Cat} , Sum = y.sum}).Take(3);
            var temp = result.ToList();
            temp.Should().NotBeEmpty();
            temp.FirstOrDefault().Sum.Should().Be(10);
        }
    }
}
