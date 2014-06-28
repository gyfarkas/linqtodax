namespace LinqToDAX.Query
{
    using System.Linq.Expressions;
    using DAXExpression;

    /// <summary>
    /// Replace expressions
    /// </summary>
    internal class Replacer : DaxExpressionVisitor
    {
        private Expression _searchFor;
        private Expression _replaceWith;

        /// <summary>
        /// visitor implementation
        /// </summary>
        /// <param name="node">node to be visited</param>
        /// <returns>transformed expression</returns>
        public override Expression Visit(Expression node)
        {
            if (node == this._searchFor)
            {
                return this._replaceWith;
            }

            return base.Visit(node);
        }

        /// <summary>
        /// Initiate the replace process
        /// </summary>
        /// <param name="exp">expression to be transformed</param>
        /// <param name="searchFor">expression to replace</param>
        /// <param name="replaceWith">expression to replace with</param>
        /// <returns>transformed expression</returns>
        internal Expression Replace(Expression exp, Expression searchFor, Expression replaceWith)
        {
            this._replaceWith = replaceWith;
            this._searchFor = searchFor;
            return this.Visit(exp);
        }
    }
}
