// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DAXExpression.cs" company="Dealogic">
//   LICENCE.md
// </copyright>
// <summary>
//   Extended expression type list to inherit from <cref ns="Linq.Expression" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LinqToDAX.Query.DAXExpression
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Extended expression type list to inherit from <cref ns="Linq.Expression"/>
    /// </summary>
    internal enum DaxExpressionType
    {
        /// <summary>
        /// Table type flag for <cref class="TableExpression"/>, number starts from 1000 to avoid clashing with the original enumeration
        /// </summary>
        Table = 1000,

        /// <summary>
        /// Type flag for columns
        /// </summary>
        Column,

        /// <summary>
        /// type flag for measures.
        /// </summary>
        Measure,

        /// <summary>
        /// Type flag
        /// </summary>
        Summarize,
        
        /// <summary>
        ///  Type flag
        /// </summary>
        AddColumns,
        
        /// <summary>
        ///  Type flag
        /// </summary>
        CalculateTable,

        /// <summary>
        ///  Type flag
        /// </summary>
        Topn,
        
        /// <summary>
        ///  Type flag
        /// </summary>
        Projection,
        
        /// <summary>
        /// Type flag
        /// </summary>
        OrderByItem,
        
        /// <summary>
        ///  Type flag
        /// </summary>
        Filter,
        
        /// <summary>
        ///  Type flag
        /// </summary>
        All,

        /// <summary>
        ///  Type flag
        /// </summary>
        Lookup,

        /// <summary>
        ///  Type flag
        /// </summary>
        XAggregation,

        /// <summary>
        ///  Type flag
        /// </summary>
        Generate,

        /// <summary>
        ///  Type flag
        /// </summary>
        Values,

        /// <summary>
        ///  Type flag
        /// </summary>
        Row,

        /// <summary>
        /// Type flag
        /// </summary>
        UseRelationship,

        /// <summary>
        /// Type flag for subQueries
        /// </summary>
        SubQuery,

        /// <summary>
        /// Type flag for grouping projection
        /// </summary>
        Grouping,
    }

    /// <summary>
    /// Simple flags for ordering directions
    /// </summary>
    internal enum OrderType
    {
        /// <summary>
        /// Ascending order
        /// </summary>
        Asc,

        /// <summary>
        /// Descending order
        /// </summary>
        Desc
    }

    /// <summary>
    /// Types of aggregations
    /// </summary>
    internal enum AggregationType
    {
        /// <summary>
        /// Sum aggregation flag
        /// </summary>
        Sum,

        /// <summary>
        /// Average aggregation flag
        /// </summary>
        Avg,

        /// <summary>
        /// Rank aggregation flag
        /// </summary>
        Rank,

        /// <summary>
        /// Min aggregation flag
        /// </summary>
        Min,

        /// <summary>
        /// Max aggregation flag
        /// </summary>
        Max,

        /// <summary>
        /// Count aggregation flag 
        /// </summary>
        Count,

        /// <summary>
        ///  Rank inverted 
        /// </summary>
        ReverseRank,

        /// <summary>
        /// Count Rows
        /// </summary>
        CountRows
    }

    /// <summary>
    /// Derived from <cref class="Linq.Expression"/> 
    /// represents DAX expression elements, 
    /// This is also an immutable type.
    /// </summary>
    public abstract class DaxExpression : Expression
    {
        /// <summary>
        /// Type of the expression usually contains <cref enum="DaxExpressionType" />
        /// </summary>
        private readonly ExpressionType _expressiontype;

        /// <summary>
        /// Actual type of the expression
        /// </summary>
        private readonly Type _type;

        /// <summary>
        /// Initializes a new instance of the <see cref="DaxExpression" /> class.
        /// </summary>
        /// <param name="expressiontype">Expression type of the expression</param>
        /// <param name="type">runtime type</param>
        protected DaxExpression(ExpressionType expressiontype, Type type)
        {
            _type = type;
            _expressiontype = expressiontype;
        }

        /// <summary>
        /// Property from <see cref="Expression" />
        /// </summary>
        public override ExpressionType NodeType
        {
            get { return _expressiontype; }
        }

        /// <summary>
        /// Property from <see cref="Expression" />
        /// </summary>
        public override Type Type
        {
            get { return _type; }
        }
    }

    /// <summary>
    /// Represents a VALUES DAX function
    /// </summary>
    internal class ValuesExpression : DaxExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValuesExpression"/> class.
        /// </summary>
        /// <param name="t">runtime type</param>
        /// <param name="column">column expression</param>
        internal ValuesExpression(Type t, ColumnDeclaration column)
            : base((ExpressionType)DaxExpressionType.Values, t)
        {
            Column = column;
        }

        /// <summary>
        /// Gets the column argument of VALUES function
        /// </summary>
        internal ColumnDeclaration Column { get; private set; }
    }

    /// <summary>
    /// Represents a VALUES DAX function
    /// </summary>
    internal class UseRelationshipExpression : DaxExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UseRelationshipExpression"/> class.
        /// </summary>
        /// <param name="t">runtime type</param>
        /// <param name="column">column expression</param>
        internal UseRelationshipExpression(ColumnExpression source, ColumnExpression target)
            : base((ExpressionType)DaxExpressionType.UseRelationship, typeof(bool))
        {
            Source = source;
            Target = target;
        }

        /// <summary>
        /// Gets the column argument of VALUES function
        /// </summary>
        internal ColumnExpression Source { get; private set; }
        internal ColumnExpression Target { get; private set; }
    }


    /// <summary>
    /// Represents a ROW DAX function call
    /// </summary>
    internal class RowExpression : DaxExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RowExpression"/> class.
        /// </summary>
        /// <param name="t">runtime type</param>
        /// <param name="measure">Measure expression argument</param>
        internal RowExpression(Type t, MeasureDeclaration measure) :
            base((ExpressionType)DaxExpressionType.Row, t)
        {
            Measure = measure;
        }

        /// <summary>
        /// Gets the measure argument of ROW function
        /// </summary>
        internal MeasureDeclaration Measure { get; private set; }
    }

    /// <summary>
    /// Represents a GENERATE function call in the expression
    /// </summary>
    internal class GenerateExpression : DaxExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateExpression"/> class.
        /// </summary>
        /// <param name="t">runtime type</param>
        /// <param name="source">source table expression</param>
        /// <param name="generator">the generating table expression</param>
        internal GenerateExpression(Type t, Expression source, Expression generator) :
            base((ExpressionType)DaxExpressionType.Generate, t)
        {
            Source = source;
            Generator = generator;
        }

        /// <summary>
        /// Gets the source table
        /// </summary>
        internal Expression Source { get; private set; }

        /// <summary>
        /// Gets the generator table
        /// </summary>
        internal Expression Generator { get; private set; }
    }

    /// <summary>
    /// Represents a column reference in the DAX expression
    /// </summary>
    internal class ColumnExpression : DaxExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnExpression"/> class. 
        /// </summary>
        /// <param name="type">runtime type</param>
        /// <param name="name">column name, used as reference</param>
        /// <param name="dbname">Database name, for the mapping</param>
        /// <param name="table">name of the table of the column</param>
        internal ColumnExpression(Type type, string name, string dbname, string table)
            : base((ExpressionType)DaxExpressionType.Column, type)
        {
            Name = name;
            DbName = dbname;
            TableName = table;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnExpression"/> class. 
        /// </summary>
        /// <param name="type">runtime type</param>
        /// <param name="et">expression type</param>
        /// <param name="name">column name, used as reference</param>
        /// <param name="dbname">Database name, for the mapping</param> 
        /// <param name="table">name of the table of the column</param>
        internal ColumnExpression(Type type, ExpressionType et, string name, string dbname, string table)
            : base(et, type)
        {
            Name = name;
            DbName = dbname;
            TableName = table;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnExpression"/> class
        /// </summary>
        /// <param name="et">Expression type</param>
        /// <param name="type">runtime type</param>
        /// <param name="name">column name</param>
        /// <param name="table">name of the table of the column</param>
        internal ColumnExpression(ExpressionType et, Type type, string name, string table) :
            base(et, type)
        {
            Name = name;
            TableName = table;
        }

        /// <summary>
        /// Gets the column's reference name
        /// </summary>
        internal string Name { get; private set; }

        /// <summary>
        /// Gets columns actual database name for mapping
        /// </summary>
        internal string DbName { get; private set; }

        /// <summary>
        /// Gets the name of the table where the column belongs 
        /// </summary>
        internal string TableName { get; private set; }
    }

    /// <summary>
    /// Represents a measure expression
    /// </summary>
    internal class MeasureExpression : ColumnExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeasureExpression"/> class. 
        /// </summary>
        /// <param name="type">runtime type</param>
        /// <param name="name">measure name</param>
        /// <param name="dbname">required by base, calculation string may go here</param>
        /// <param name="alias">reference name when measure is lifted to define section</param>
        /// <param name="table">the name of the table in which the measure can be defined</param>
        internal MeasureExpression(Type type, string name, string dbname, string alias, string table) :
            base(type, (ExpressionType) DaxExpressionType.Measure, name, dbname, table)
        {
            Alias = alias;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasureExpression"/> class. 
        /// </summary>
        /// <param name="type">runtime type</param>
        /// <param name="et">Expression type</param>
        /// <param name="name">measure name</param>
        /// <param name="dbname">required by base, calculation string may go here</param>
        /// <param name="alias">reference name when measure is lifted to define section</param>
        /// <param name="table">the name of the table in which the measure can be defined</param>
        /// <param name="filter">filter expression to restrict the calculation internally</param>
        internal MeasureExpression(Type type, ExpressionType et,  string name, string dbname, string alias, string table, Expression filter) :
            base(type, et, name, dbname, table)
        {
            Filter = filter;
            Alias = alias;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasureExpression"/> class. 
        /// </summary>
        /// <param name="et">Expression type</param>
        /// <param name="type">runtime type</param>
        /// <param name="name">measure name</param>
        /// <param name="alias">reference name when measure is lifted to define section</param>
        /// <param name="table">the name of the table in which the measure can be defined</param>
        internal MeasureExpression(ExpressionType et, Type type, string name, string alias, string table) : base(et, type, name, table)
        {
            Alias = alias;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasureExpression"/> class. 
        /// </summary>
        ///  <param name="expressionType">Expression type</param>
        /// <param name="t">runtime type</param>
        /// <param name="name">measure name</param>
        /// <param name="dbstring">tabular expression for the measure</param>
        internal MeasureExpression(ExpressionType expressionType, Type t, string name, string dbstring) :
            base(t, expressionType, name, dbstring, null)
        {}

        /// <summary>
        /// Gets filter expression for restricting the calculation
        /// </summary>
        internal Expression Filter { get; private set; }

        /// <summary>
        /// Gets the Alias used in define section of the query
        /// </summary>
        internal string Alias { get; private set; }
    }


    internal class LookupExpression : MeasureExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LookupExpression"/> class
        /// </summary>
        /// <param name="t">runtime type</param>
        /// <param name="target">lookup value</param>
        /// <param name="lookupColumns">lookup criteria</param>
        /// <param name="dbstring">actual tabular expression</param>
        internal LookupExpression(Type t, ColumnExpression target, List<Tuple<ColumnExpression, ColumnExpression>> lookupColumns, string dbstring)
            : base((ExpressionType)DaxExpressionType.Lookup, t, "[" + target.Name + "]", dbstring)
        {
            LookupColumns = lookupColumns;
        }

        /// <summary>
        /// Gets the lookup columns for the expression
        /// </summary>
        internal List<Tuple<ColumnExpression, ColumnExpression>> LookupColumns { get; private set; }
    }


    /// <summary>
    /// The aggregation expression. (for functions like SUMX)
    /// </summary>
    internal class XAggregationExpression : MeasureExpression
    {

        internal XAggregationExpression(
            AggregationType aggregationType, 
            Expression source, 
            ColumnExpression column,
            string name,
            string table,
            Type type)
            : base((ExpressionType)DaxExpressionType.XAggregation, type, name, name, table)
        {
            Source = source;
            Column = column;
            AggregationType = aggregationType;
        }

        internal XAggregationExpression(
            AggregationType aggregationType, 
            Expression source, 
            ColumnExpression column,
            string name,
            string table,
            Type type, 
            Expression filter)
            : base(type, (ExpressionType)DaxExpressionType.XAggregation, name, name, name, table, filter)
        {
            Source = source;
            Column = column;
            AggregationType = aggregationType;
        }

        internal XAggregationExpression(
            AggregationType aggregationType,
            Expression source,
            Expression complex,
            string name,
            string table,
            Type type)
            : base((ExpressionType)DaxExpressionType.XAggregation, type, name, name, table)
        {
            Source = source;
            ComplexExpression = complex;
            AggregationType = aggregationType;
        }

        internal AggregationType AggregationType { get; private set; }
        internal Expression Source { get; private set; }
        internal ColumnExpression Column { get; private set; }
        internal Expression ComplexExpression { get; set; }
    }

    internal class ColumnDeclaration
    {
        internal ColumnDeclaration(string name, string alias, Expression exp)
        {
            Name = name;
            Alias = alias;
            Expression = exp;
        }

        internal string Alias { get; private set; }
        internal string Name { get; private set; }
        internal Expression Expression { get; private set; }
    }

    internal class MeasureDeclaration : ColumnDeclaration
    {
        internal MeasureDeclaration(string name, string alias, Expression exp) :
            base(name, alias, exp)
        {
        }
    }

    internal class SummarizeExpression : DaxExpression
    {
        internal SummarizeExpression(Type t, IEnumerable<ColumnDeclaration> columns, Expression maintable)
            : base((ExpressionType) DaxExpressionType.Summarize, t)
        {
            AllColumns = columns as ReadOnlyCollection<ColumnDeclaration>;
            MainTable = maintable;

            if (AllColumns == null && columns != null)
            {
                AllColumns = new List<ColumnDeclaration>(columns).AsReadOnly();
            }
        }

        internal ReadOnlyCollection<ColumnDeclaration> AllColumns { get; private set; }

        internal Expression MainTable { get; private set; }

        internal IEnumerable<ColumnDeclaration> Columns
        {
            get { return AllColumns.Where(x => !(x.Expression is MeasureExpression)); }
        }

        internal IEnumerable<ColumnDeclaration> Measures
        {
            get { return AllColumns.Where(x => x.Expression is MeasureExpression); }
        }
    }

    /// <summary>
    /// Expression type corresponding to ADDCOLUMNS call in a DAX query
    /// </summary>
    internal class AddColumnsExpression : DaxExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddColumnsExpression"/> class. 
        /// </summary>
        /// <param name="t">runtime type</param>
        /// <param name="columns">column declarations added to the query</param>
        /// <param name="maintable">base table</param>
        internal AddColumnsExpression(Type t, IEnumerable<ColumnDeclaration> columns, Expression maintable)
            : base((ExpressionType) DaxExpressionType.AddColumns, t)
        {
            Columns = columns as ReadOnlyCollection<ColumnDeclaration>;
            MainTable = maintable;
            if (Columns == null)
            {
                Columns = new List<ColumnDeclaration>(columns).AsReadOnly();
            }
        }

        /// <summary>
        /// Gets Column/measure declarations included 
        /// </summary>
        internal ReadOnlyCollection<ColumnDeclaration> Columns { get; private set; }

        /// <summary>
        /// Gets the source table of the query
        /// </summary>
        internal Expression MainTable { get; private set; }
    }

    internal class CalculateTableExpression : DaxExpression
    {
        internal CalculateTableExpression(Type t, Expression maintable, Expression where)
            : base((ExpressionType) DaxExpressionType.CalculateTable, t)
        {
            MainTable = maintable;
            Filters = where;
        }

        internal Expression MainTable { get; private set; }
        internal Expression Filters { get; private set; }
    }

    internal class FilterExpression : DaxExpression
    {
        internal FilterExpression(Type resultType, Expression source, Expression where, IEnumerable<ColumnDeclaration> columns )
            : base((ExpressionType) DaxExpressionType.Filter, resultType)
        {
            MainTable = source;
            Filter = where;
            Columns = columns.ToList().AsReadOnly();
        }

        internal Expression MainTable { get; private set; }
        internal Expression Filter { get; private set; }
        internal ReadOnlyCollection<ColumnDeclaration> Columns { get; set; }
    }

    internal class TopnExpression : DaxExpression
    {
        internal TopnExpression(Type t, Expression maintable, Expression orderby, int top)
            : base((ExpressionType) DaxExpressionType.Topn, t)
        {
            MainTable = maintable;
            OrderBy = orderby;
            Top = top;
        }

        internal Expression MainTable { get; private set; }
        internal int Top { get; private set; }
        internal Expression OrderBy { get; private set; }
    }


    internal class OrderByExpression : DaxExpression
    {
        internal OrderByExpression(Type t, ColumnExpression col, OrderType orderType)
            : base((ExpressionType) DaxExpressionType.OrderByItem, t)
        {
            OrderColumn = col;
            OrderType = orderType;
        }

        internal ColumnExpression OrderColumn { get; private set; }
        internal OrderType OrderType { get; private set; }
    }


    internal class ProjectionExpression : DaxExpression
    {
        internal ProjectionExpression(DaxExpression source, Expression projector)
            : base((ExpressionType)DaxExpressionType.Projection, projector.Type)
        {
            Source = source;
            Projector = projector;
        }

        internal ProjectionExpression(Type t, DaxExpression source, Expression projector) :
            base((ExpressionType) DaxExpressionType.Projection, t)
        {
            Source = source;
            Projector = projector;
        }

        internal DaxExpression Source { get; private set; }
        internal Expression Projector { get; private set; }
    }
}