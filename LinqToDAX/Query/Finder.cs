// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Finder.cs" company="Dealogic">
//   See LICENCE.md
// </copyright>
// <summary>
//   Finds the expressions of the specified type
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LinqToDAX.Query
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using LinqToDAX.Query.DAXExpression;

    /// <summary>
    /// Finds the expressions of the specified type
    /// </summary>
    /// <typeparam name="T">an expression type that we want to search for</typeparam>
    internal class Finder<T> : DaxExpressionVisitor where T : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Finder{T}"/> class
        /// </summary>
        internal Finder()
        {
            Expressions = new List<T>();
        }

        /// <summary>
        /// Gets a value indicating whether the specified expression type was found in the node.
        /// </summary>
        internal bool Found { get; private set; }

        /// <summary>
        /// Gets the first expression found of type T from the node
        /// </summary>
        internal T First { get; private set; }

        /// <summary>
        /// Gets all found expressions 
        /// </summary>
        internal List<T> Expressions { get; private set; }

        /// <summary>
        /// Visits the node and searches for the given type sub-expression 
        /// </summary>
        /// <param name="node">expression to be examined</param>
        /// <returns>the original expression</returns>
        public override Expression Visit(Expression node)
        {
            if (node is T)
            {
                if (!Found)
                {
                    First = (T)node;
                }

                Expressions.Add((T)node);
                Found = true;
            }

            return base.Visit(node);
        }

        /// <summary>
        /// Decide whether the node contains a sub-expression of the specified type 
        /// </summary>
        /// <param name="node">expression to be inspected</param>
        /// <returns>boolean value indicating whether the expression has a sub-expression of the specified type </returns>
        internal bool Has(Expression node)
        {
            Visit(node);
            return Found;
        }
    }
}