using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqToDAX
{
    /// <summary>
    /// Attribute to store and retrieve Tabular specific information for query generation
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class TabularMappingAttribute : System.Attribute
    {
        public string ColumnName { get; set; }
        public string TableName { get; set; }

      

        public TabularMappingAttribute(string name, string table)
        {
            ColumnName = name;
            TableName = table;
        }

        public TabularMappingAttribute(string name)
        {
            ColumnName = name;
        }

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class TabularTableMappingAttribute : System.Attribute
    {
        public string TableName { get; set; }

        public TabularTableMappingAttribute(string name)
        {
            TableName = name;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class TabularMeasureMappingAttribute : System.Attribute
    {
        public string MeasureName { get; set; }
        public TabularMeasureMappingAttribute(string name)
        {
            MeasureName = name;
        }
    }
}
