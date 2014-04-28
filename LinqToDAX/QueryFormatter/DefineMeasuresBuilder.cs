

using System;

namespace LinqToDAX.QueryFormatter
{
    using System.Linq.Expressions;
    using Query.DAXExpression;

    /// <summary>
    /// Build the measure definition string for a measure
    /// </summary>
    internal class DefineMeasuresBuilder : TabularQueryFormatter
    {
        /// <summary>
        /// Format measure definition
        /// </summary>
        /// <param name="measure">measure expression</param>
        /// <returns>the original expression</returns>
        public override Expression Visit(Expression measure)
        {
            // Don't generate measure for lookups
            if ((DaxExpressionType)measure.NodeType == DaxExpressionType.Lookup)
            {
                base.Visit(measure);
            }

            var m = measure as MeasureExpression;
            
            if (m != null && !string.IsNullOrEmpty(m.TableName))
            {
                this.Builder.Append("MEASURE\n");
                if (m is XAggregationExpression)
                {
                    this.Builder.Append(m.TableName + m.Name + "=" + "CALCULATE(");
                    this.VisitXAggregation(m as XAggregationExpression);
                    this.Builder.Append(")\n");
                }
                else if (m.Filter != null)
                {
                    this.Builder.Append(m.TableName + m.Name + "=" + "CALCULATE(" + m.DbName + ",");
                    this.Visit(m.Filter);
                    this.Builder.Append(")\n");
                }
                else if (m.DbName != null)
                {
                    this.Builder.Append(m.TableName +
                                        m.Name + 
                                        "=" + 
                                        "CALCULATE(" +
                                        m.DbName +
                                        ")\n");
                }
                else
                {
                    this.Builder.Append(m.Name);
                }
            }
            else
            {
                base.Visit(measure);
            }

            return measure;
        }

        /// <summary>
        /// Gets the definition string for a measure
        /// </summary>
        /// <param name="measure">measure expression</param>
        /// <returns>definition string for the define section of the query</returns>
        internal string GetDefinition(Expression measure)
        {
            Visit(measure);
            return this.Builder.ToString();
        }

        /// <summary>
        /// Generate query string part from aggregation expression
        /// </summary>
        /// <param name="aggregationExpression">input aggregation expression</param>
        /// <returns>the expression unchanged</returns>
        protected override Expression VisitXAggregation(XAggregationExpression aggregationExpression)
        {
            var aggregationString = new XaggregationVisitor().GetAggragationString(aggregationExpression);
            this.Builder.Append(aggregationString);
            return aggregationExpression;
        }
    }
}
