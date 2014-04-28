// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XAggregationVisitor.cs" company="Dealogic">
//   See LICENCE.md
// </copyright>
// <summary>
//   Special formatting for XAggregationExpression
// </summary>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using LinqToDAX.Query.DAXExpression;

namespace LinqToDAX.QueryFormatter
{
    using System.Linq.Expressions;
    using Query;

    /// <summary>
    /// Special formatting for XAggregationExpression
    /// </summary>
    internal class XaggregationVisitor : TabularQueryFormatter
    {
        /// <summary>
        /// Generates query string corresponding to an aggregation expression (like "SUMX")
        /// </summary>
        /// <param name="aggregationExpression">aggregation expression</param>
        /// <returns>query string part</returns>
        public string GetAggragationString(XAggregationExpression aggregationExpression)
        {
            Visit(aggregationExpression);
            return Builder.ToString();
        }

        /// <summary>
        /// Table expression formatter within aggregations
        /// </summary>
        /// <param name="tableExpression">table expression</param>
        /// <returns>the original expression</returns>
        protected override Expression VisitTable(TableExpression tableExpression)
        {
            Builder.Append("RELATEDTABLE(");
            Builder.Append(tableExpression.Name);
            Builder.Append(")");
            return tableExpression;
        }

        /// <summary>
        /// Format aggregation expression
        /// </summary>
        /// <param name="aggregationExpression">aggregation expression to be formatted in a query string</param>
        /// <returns>the original expression</returns>
        protected override Expression VisitXAggregation(XAggregationExpression aggregationExpression)
        {
            switch (aggregationExpression.AggregationType)
            {
                case AggregationType.Sum:
                    Builder.Append("SUMX(");
                    break;
                case AggregationType.Avg:
                    Builder.Append("AVERAGEX(");
                    break;
                case AggregationType.Count:
                    Builder.Append("COUNTAX(");
                    break;
                case AggregationType.Max:
                    Builder.Append("MAXX(");
                    break;
                case AggregationType.Min:
                    Builder.Append("MINX(");
                    break;
                case AggregationType.Rank:
                case AggregationType.ReverseRank:
                    Builder.Append("RANKX(");
                    break;
            }

            Visit(aggregationExpression.Source);
            Builder.Append(",\n");
            if (aggregationExpression.AggregationType == AggregationType.ReverseRank)
            {
                Builder.Append("-");
            }

            if (aggregationExpression.Column is MeasureExpression)
            {
                Builder.Append(aggregationExpression.Column.Name);
            }
            else
            {
                Builder.Append(aggregationExpression.Column.DbName);
            }

            Builder.Append(")");
            return aggregationExpression;
        }

        /// <summary>
        /// Format AddColumns expression
        /// </summary>
        /// <param name="addColumnsExpression">input expression to be formatted</param>
        /// <returns>the expression unchanged</returns>
        protected override Expression VisitAddColumns(AddColumnsExpression addColumnsExpression)
        {
            Builder.Append("ADDCOLUMNS(");
            Visit(addColumnsExpression.MainTable);
            VisitMeasures(addColumnsExpression.Columns); 
            Builder.Append(")");
            return addColumnsExpression;
        }



        /// <summary>
        /// Format measures 
        /// </summary>
        /// <param name="measures">collection of measures declarations</param>
        protected override void VisitMeasures(IEnumerable<ColumnDeclaration> measures)
        {
            IEnumerable<MeasureExpression> ms = measures.Select(c => ((MeasureExpression)c.Expression));

            foreach (MeasureExpression m in ms)
            {
                this.Builder.Append(",");
                FormatMeasure(m);
            }
        }
    }
}

