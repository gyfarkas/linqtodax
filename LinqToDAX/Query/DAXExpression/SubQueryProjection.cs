namespace LinqToDAX.Query.DAXExpression
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Decorator class for subordinate queries 
    /// </summary>
    internal class SubQueryProjection : DaxExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubQueryProjection"/> class.
        /// </summary>
        /// <param name="type">runtime type of projection</param>
        /// <param name="exp">Projection to be decorated as subordinate</param>
        internal SubQueryProjection(Type type, ProjectionExpression exp)
            : base((ExpressionType)DaxExpressionType.SubQuery, type)
        {
            this.Projection = exp;
        }

        /// <summary>
        /// Gets the captured projection expression
        /// </summary>
        internal ProjectionExpression Projection { get; private set; }

    }
}
