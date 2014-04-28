using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AnalysisServices;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class AmomdDiscoverTests
    {
        [Test]
        public void Test()
        {
            Server server = new Server();
            server.Connect("localhost");
            Database db = server.Databases["AdventureWorks Tabular Model SQL 2012"];

            var measures = db.Cubes[0].MdxScripts[0];

            var c = measures.CalculationProperties.GetEnumerator();
                while(c.MoveNext())
                {
                    CalculationProperty cp = (CalculationProperty) c.Current;
                    var type = cp.FormatString;
                    
                    System.Console.WriteLine(cp.CalculationReference + "  " + type);
                    
                    //System.Console.WriteLine(cp.Container.ToString());
                    
                }
        }

        [Test]
        public void ReadCsdl()
        {
            var doc = XDocument.Load(XmlReader.Create(new StreamReader(@"C:\Users\Gyorgy\documents\visual studio 2013\Projects\LinqToDAX\Adventureworks\schema.csdl")));
            var entities = 
                from elem in doc.Root.Descendants()
                where elem.Name.LocalName == "EntitySet" 
                select new
                {
                    Node = elem,
                    Childeren = elem.Elements()
                };
            entities.ToList().ForEach(x => Console.WriteLine(x));



        }

    }
}

