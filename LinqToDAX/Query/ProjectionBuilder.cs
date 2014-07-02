// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectionBuilder.cs" company="Dealogic">
//   see LICENCE.md
// </copyright>
// <summary>
//   This class builds up the reader instance via reflection
//   that is used to get the data from the database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LinqToDAX.Query
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using DAXExpression;


    internal class SubQueryProjectionBuilder : ProjectionBuilder
    {
        /// <summary>
        /// When analysing binary expressions in subqueries we only evaluate the right operand
        /// </summary>
        /// <param name="node">binary expression</param>
        /// <returns>converted expression</returns>
        
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.Equal)
            {
                var left = node.Left;
                var right = Visit(node.Right);
                return Expression.MakeBinary(node.NodeType, left, right);
            }

            return base.VisitBinary(node);
        }
    }

    /// <summary>
    /// This class builds up the reader instance via reflection
    ///  that is used to get the data from the database.
    /// </summary>
    /// 
    internal class ProjectionBuilder : DaxExpressionVisitor
    {
        /// <summary>
        /// The reflected method to read from the DataReader
        /// </summary>
        private static MethodInfo _getValue;

        /// <summary>
        /// Reflected method to execute a subordinate query expression
        /// </summary>
        private static MethodInfo _executeSubQuery;

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
                _executeSubQuery = typeof(ProjectionRow).GetMethod("ExecuteSubQuery");
            }
            _row = Expression.Parameter(typeof(ProjectionRow), "row");
        }

        /// <summary>
        /// Builds new lambda expression with the given expression as body.
        /// </summary>
        /// <param name="exp">body expression</param>
        /// <returns>lambda expression</returns>
        internal LambdaExpression Build(Expression exp)
        {
            Expression body = Visit(exp);
            return Expression.Lambda(body, _row);
        }

        /// <summary>
        /// Get embedded projections
        /// </summary>
        /// <param name="projectionExpression">projection expression</param>
        /// <returns>projector expression</returns>
        protected override Expression VisitProjection(ProjectionExpression projectionExpression)
        {
            return Visit(projectionExpression.Projector);
        }    

        /// <summary>
        /// Transform subordinate queries
        /// </summary>
        /// <param name="projectionExpression">subordinate query</param>
        /// <returns>converted projection</returns>
        protected override Expression VisitSubQuery(SubQueryProjection projectionExpression)
        {
            var exp = (DaxExpression)new SubQueryProjectionBuilder().Visit(projectionExpression.Projection.Source);
            var projection = new ProjectionExpression(exp, projectionExpression.Projection.Projector);
            var q = new SubQueryProjection(projectionExpression.Type, projection);
            LambdaExpression subQuery = Expression.Lambda(q, _row);
            if (projectionExpression.Type.IsGenericType)
            {
                Type elementType = TypeSystem.GetElementType(subQuery.Body.Type);
                MethodInfo mi = _executeSubQuery.MakeGenericMethod(elementType);
                return Expression.Convert(Expression.Call(_row, mi, Expression.Constant(subQuery)), projectionExpression.Type);
            }

            return base.Visit(projectionExpression);
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
        protected static string MeasureNameReference(string name)
        {
            var n = name.StartsWith("[") ? name : "[" + name;
            return n.EndsWith("]") ? n : n + "]";
        }
    }
}