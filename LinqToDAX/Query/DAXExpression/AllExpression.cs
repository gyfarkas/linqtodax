// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AllExpression.cs" company="Dealogic">
//   see LICENCE.md
// </copyright>
// <summary>
//   Types of ALL and ALLSELECTED expressions
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LinqToDAX.Query.DAXExpression
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Types of ALL and ALLSELECTED expressions
    /// </summary>
    internal enum AllType
    {
        /// <summary>
        /// Type flag denoting ALLSELECTED 
        /// </summary>
        AllSelected,

        /// <summary>
        /// Type flag denoting ALL
        /// </summary>
        All
    }

    /// <summary>
    /// Class representing ALL DAX function expressions
    /// </summary>
    internal class AllExpression : DaxExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AllExpression"/> class. 
        /// </summary>
        /// <param name="t">runtime type</param>
        /// <param name="arg">argument for the ALL DAX function</param>
        public AllExpression(Type t, Expression arg) : base((ExpressionType)DaxExpressionType.All, t)
        {
            Argument = arg;
        }

        /// <summary>
        /// Gets the argument expression 
        /// </summary>
        internal Expression Argument { get; private set; }
    }

    /// <summary>
    /// Class representing ALLSELECTED function call
    /// </summary>
    internal class AllSelectedExpression : AllExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AllSelectedExpression"/> class.
        /// </summary>
        /// <param name="t">runtime type</param>
        /// <param name="arg">argument expression of the function</param>
        public AllSelectedExpression(Type t, Expression arg)
            : base(t, arg)
        {
        }
    }
}