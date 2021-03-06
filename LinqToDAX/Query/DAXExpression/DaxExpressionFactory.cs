// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DaxExpressionFactory.cs" company="Dealogic">
//   LICENCE.md
// </copyright>
// <summary>
//   Extended expression type list to inherit from <cref ns="Linq.Expression" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.ObjectModel;
using System.Reflection;

namespace LinqToDAX.Query.DAXExpression
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Factory for complex DAX expressions 
    /// </summary>
    internal static class DaxExpressionFactory
    {
        /// <summary>
        /// Static factory method to recursively create summarize or optimized forms (values, add columns, row expressions) 
        /// </summary>
        /// <param name="type">runtime type</param>
        /// <param name="columns">column declarations</param>
        /// <param name="maintable">source table</param>
        /// <returns>DAX expression</returns>
        internal static DaxExpression Create(Type type, IEnumerable<ColumnDeclaration> columns, Expression maintable)
        {
            Expression main;
            Expression where;
            switch ((DaxExpressionType)maintable.NodeType)
            {
                case DaxExpressionType.CalculateTable:
                    main = Create(type, columns.Distinct(), ((CalculateTableExpression)maintable).MainTable);
                    @where = ((CalculateTableExpression)maintable).Filters;
                    return new CalculateTableExpression(type, main, @where);
                case DaxExpressionType.Filter:
                    var filterColumns = ((FilterExpression) maintable).Columns;
                    main = Create(type, columns.Union(filterColumns).Distinct(), ((FilterExpression)maintable).MainTable);
                    @where = ((FilterExpression)maintable).Filter;
                    return new FilterExpression(type, main, @where, ((FilterExpression)maintable).Columns);
            }

            var columnList = columns.ToList();
            var measures = columnList.Where(x => x is MeasureDeclaration).ToList();

            switch ((DaxExpressionType)maintable.NodeType)
            {
                case DaxExpressionType.Summarize:
                    var cs = ((SummarizeExpression)maintable).Columns;
                    maintable = ((SummarizeExpression)maintable).MainTable;

                    columnList = columnList.Union(cs).ToList();
                    break;
                case DaxExpressionType.AddColumns:
                    var cs1 = ((AddColumnsExpression)maintable).Columns.Select(c => c);
                    measures = cs1.ToList();
                    maintable = ((AddColumnsExpression)maintable).MainTable;
                    columnList = columnList.Union(measures).ToList();
                    break;
            }
            if (columnList.Distinct().Count() == 1)
            {
                var theColumn = columnList.FirstOrDefault();
                if (theColumn is MeasureDeclaration)
                {
                    return new RowExpression(type, (MeasureDeclaration)theColumn);
                }
                
                if (theColumn != null)
                {
                    return new ValuesExpression(type, theColumn);
                }
            }

            if (measures.Any())
            {
               
                var summarize = Create(
                    type,
                    columnList.Where(c => !(c is MeasureDeclaration)).Distinct(),
                    maintable);

                if ((DaxExpressionType)maintable.NodeType == DaxExpressionType.Summarize && ((SummarizeExpression)maintable).AllColumns.Any())
                {
                    return new AddColumnsExpression(type, measures.Distinct(), maintable);  
                }

                return new AddColumnsExpression(type, measures.Distinct(), summarize);  
            }
            var maint = maintable as SummarizeExpression;

            if (maint != null && (DaxExpressionType)maintable.NodeType == DaxExpressionType.Summarize)
            {
                return new SummarizeExpression(type, columnList.Distinct(), maint.MainTable);
            }

            return new SummarizeExpression(type, columnList.Distinct(), maintable);
        }
    }
}