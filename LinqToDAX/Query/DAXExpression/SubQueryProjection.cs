using System;
using System.Linq.Expressions;

namespace LinqToDAX.Query.DAXExpression
{
    /// <summary>
    /// Decorator class for subordinate queries 
    /// </summary>
    internal class SubQueryProjection : DaxExpression
    {
        internal ProjectionExpression Exp { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubQueryProjection"/> class.
        /// </summary>
        /// <param name="proj">Projection to be decorated as subordinate</param>
        internal SubQueryProjection(Type type, ProjectionExpression exp)
            : base((ExpressionType)DaxExpressionType.SubQuery, type)
        {
            this.Exp = exp;
        }
    }
}
