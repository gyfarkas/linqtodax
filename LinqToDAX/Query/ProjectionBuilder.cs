﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectionBuilder.cs" company="Dealogic">
//   see LICENCE.md
// </copyright>
// <summary>
//   This class builds up the reader instance via reflection
//   that is used to get the data from the database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using LinqToDAX.Query.DAXExpression;

namespace LinqToDAX.Query
{
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// This class builds up the reader instance via reflection
    ///  that is used to get the data from the database.
    /// </summary>
    internal class ProjectionBuilder : DaxExpressionVisitor
    {
        /// <summary>
        /// The reflected method to read from the DataReader
        /// </summary>
        private static MethodInfo _getValue;

        /// <summary>
        /// parameter expression used for the compiled lambda expression
        /// </summary>
        private ParameterExpression _row;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectionBuilder"/> class.
        /// </summary>
        internal ProjectionBuilder()
        {
            if (_getValue == null)
            {
                _getValue = typeof(ProjectionRow).GetMethod("GetValue");
            }
        }

        /// <summary>
        /// Builds new lambda expression with the given expression as body.
        /// </summary>
        /// <param name="exp">body expression</param>
        /// <returns>lambda expression</returns>
        internal LambdaExpression Build(Expression exp)
        {
            _row = Expression.Parameter(typeof(ProjectionRow), "row");
            Expression body = Visit(exp);
            return Expression.Lambda(body, _row);
        }

        /// <summary>
        /// Handles Projection expression projector part
        /// </summary>
        /// <param name="projectionExpression">projection expression to visit</param>
        /// <returns>processed projector</returns>
        protected override Expression VisitProjection(ProjectionExpression projectionExpression)
        {
            return Visit(projectionExpression.Projector);
        }

        /// <summary>
        /// creates a call to get value of an aggregation function like "SUMX" etc.
        /// </summary>
        /// <param name="aggregationExpression">the aggregation function expression</param>
        /// <returns>method call to get value</returns>
        protected override Expression VisitXAggregation(XAggregationExpression aggregationExpression)
        {
            string measureProjectionName = aggregationExpression.Name;
            var expressions =
                new Expression[]
                {
                    Expression.Constant(measureProjectionName),
                    Expression.Constant(aggregationExpression.Type)
                };
            return Expression.Convert(Expression.Call(_row, _getValue, expressions), aggregationExpression.Type);
        }

        /// <summary>
        /// creates a method call to get the value from a column
        /// </summary>
        /// <param name="column">column reference expression</param>
        /// <returns>get value method call</returns>
        protected override Expression VisitColumn(ColumnExpression column)
        {
            if (column is MeasureExpression)
            {
                return VisitMeasure((MeasureExpression)column);
            }

            string projectionName = column.DbName.Replace("'", string.Empty);
            var projectionExpressions =
                new Expression[]
                {
                    Expression.Constant(projectionName),
                    Expression.Constant(column.Type)
                };
            return Expression.Convert(Expression.Call(_row, _getValue, projectionExpressions), column.Type);
        }

        /// <summary>
        /// creates method call to get value from lookup columns
        /// </summary>
        /// <param name="lookup">measure expression</param>
        /// <returns>get value method call expression</returns>
        protected override Expression VisitLookup(LookupExpression lookup)
        {
            string measureProjectionName = MeasureNameReference(lookup.Name);
            var measureExpressions =
                new Expression[]
                    {
                        Expression.Constant(measureProjectionName),
                        Expression.Constant(lookup.Type)
                    };
            return Expression.Convert(Expression.Call(_row, _getValue, measureExpressions), lookup.Type);
        }


        /// <summary>
        /// creates method call to get value from measure columns
        /// </summary>
        /// <param name="measure">measure expression</param>
        /// <returns>get value method call expression</returns>
        protected override Expression VisitMeasure(MeasureExpression measure)
        {
            string measureProjectionName = MeasureNameReference(measure.Name);
            var measureExpressions =
                new Expression[]
                    {
                        Expression.Constant(measureProjectionName),
                        Expression.Constant(measure.Type)
                    };
            return Expression.Convert(Expression.Call(_row, _getValue, measureExpressions), measure.Type);
        }

        /// <summary>
        /// Method to create a measure name reference
        /// </summary>
        /// <param name="name">basic measure name</param>
        /// <returns>correctly bracketed measure reference name</returns>
        private static string MeasureNameReference(string name)
        {
            var n = name.StartsWith("[") ? name : "[" + name;
            return n.EndsWith("]") ? n : n + "]";
        }
    }
}