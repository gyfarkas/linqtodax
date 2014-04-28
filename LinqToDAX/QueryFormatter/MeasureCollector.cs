// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeasureCollector.cs" company="Dealogic">
//   see LICENCE.md
// </copyright>
// <summary>
//   Compiles a dictionary of measures to use for formatting the query string and generating the define section
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LinqToDAX.QueryFormatter
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;
    using Query.DAXExpression;

    /// <summary>
    /// Compiles a dictionary of measures to use for formatting the query string and generating the define section
    /// </summary>
    internal class MeasureCollector : DaxExpressionVisitor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeasureCollector"/> class.
        /// </summary>
        internal MeasureCollector()
        {
            this.Measures = new List<MeasureExpression>();
        }

        /// <summary>
        /// Gets or sets the internal map 
        /// </summary>
        private List<MeasureExpression> Measures { get; set; }

        /// <summary>
        /// Collects information on measures
        /// </summary>
        /// <param name="node">expression containing the measure declarations</param>
        /// <returns>map of measures</returns>
        internal IEnumerable<MeasureExpression> Collect(Expression node)
        {
            Visit(node);
            return this.Measures;
        }

        /// <summary>
        /// Fills the map from the column declarations
        /// </summary>
        /// <param name="readOnlyCollection">column declarations</param>
        /// <returns>the original collection of declarations</returns>
        protected override ReadOnlyCollection<ColumnDeclaration> VisitColumnDeclarations(ReadOnlyCollection<ColumnDeclaration> readOnlyCollection)
        {
            foreach (var columnDeclaration in readOnlyCollection)
            {
                var measureDeclaration = columnDeclaration as MeasureDeclaration;
                if (measureDeclaration != null)
                {
                    var m = measureDeclaration.Expression as MeasureExpression;
                    if (m != null && !string.IsNullOrEmpty(m.TableName))
                    {
                        Measures.Add(m);
                    }
                }
            }

            return readOnlyCollection;
        }

        /// <summary>
        /// Add measure in row expression to the map
        /// </summary>
        /// <param name="node">Row expression</param>
        /// <returns>the original row expression</returns>
        protected override Expression VisitRow(RowExpression node)
        {
            Measures.Add((MeasureExpression)node.Measure.Expression);
            return node;
        }
    }
}
