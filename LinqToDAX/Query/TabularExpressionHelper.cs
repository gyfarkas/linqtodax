namespace LinqToDAX.Query
{
    using System.Linq.Expressions;
    using LinqToDAX.Query.DAXExpression;

    /// <summary>
    /// General utilities to handle expressions
    /// </summary>
    internal class TabularExpressionHelper
    {
        /// <summary>
        /// Determines whether an expression corresponds to a column
        /// </summary>
        /// <param name="node">expression to be inspected</param>
        /// <returns>boolean expressing whether the expression is a column</returns>
        internal static bool CanBeColumn(Expression node)
        {
            return node.NodeType == (ExpressionType)DaxExpressionType.Column ||
                   node.NodeType == (ExpressionType)DaxExpressionType.Measure ||
                   node.NodeType == (ExpressionType)DaxExpressionType.Lookup ||
                   node.NodeType == (ExpressionType)DaxExpressionType.XAggregation;
        }

        /// <summary>
        /// Removes quotes
        /// </summary>
        /// <param name="e">quoted expression</param>
        /// <returns>expression without quotes</returns>
        internal static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = (e as UnaryExpression) != null ? (e as UnaryExpression).Operand : e;
            }

            return e;
        }
    }
}