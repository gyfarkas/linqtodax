// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FilterVisitor.cs" company="Dealogic">
//   see LICENCE.md
// </copyright>
// <summary>
//   Special formatter class for  filter expressions
// </summary>
// --------------------------------------------------------------------------------------------------------------------


namespace LinqToDAX.QueryFormatter
{

    using System.Linq.Expressions;
    using LinqToDAX.Query;
    /// <summary>
    /// Special formatter class for  filter expressions
    /// </summary>
    internal class FilterVisitor : TabularQueryFormatter
    {
        /// <summary>
        /// method to generate filter string
        /// </summary>
        /// <param name="filter">filter expression to be formatted</param>
        /// <returns>the query string part corresponding to the filter expression</returns>
        public string GetFilterString(Expression filter)
        {
            Visit(filter);
            return Builder.ToString();
        }

        /// <summary>
        /// Formatting binary expressions are special in filter expressions
        /// </summary>
        /// <param name="node">the binary expression to be formatted</param>
        /// <returns>the original expression</returns>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            base.Visit(node.Left);
            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                    Builder.Append(" = ");
                    break;
                case ExpressionType.NotEqual:
                    Builder.Append(" <> ");
                    break;
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    Builder.Append(" && ");
                    break;
                case ExpressionType.LessThan:
                    Builder.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    Builder.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    Builder.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    Builder.Append(" >= ");
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    Builder.Append("||");
                    break;
                default:
                    throw new TabularException("OR queries are not supported yet, try to rephrase your query");
            }

            base.Visit(node.Right);

            return node;
        }

    }

}
