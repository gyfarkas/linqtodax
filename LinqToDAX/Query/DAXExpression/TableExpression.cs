// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TableExpression.cs" company="Dealogic">
//   see LICENCE.md
// </copyright>
// <summary>
//   Represents a table reference within the query
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LinqToDAX.Query.DAXExpression
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Represents a table reference within the query
    /// </summary>
    internal class TableExpression : DaxExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableExpression"/> class.
        /// </summary>
        /// <param name="type">runtime type</param>
        /// <param name="name">table name</param>
        internal TableExpression(Type type, string name)
            : base((ExpressionType)DaxExpressionType.Table, type)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the table name
        /// </summary>
        internal string Name { get; private set; }
    }
}