// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TabularQueryFormatter.cs" company="Dealogic">
//   see LICENSE.md
// </copyright>
// <summary>
//   This class is responsible to assemble the actual DAX query string
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LinqToDAX.QueryFormatter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using Query.DAXExpression;

    ///<summary>
    /// This class is responsible to assemble the actual DAX query string
    ///</summary>
    internal class TabularQueryFormatter : DaxExpressionVisitor
    {
        /// <summary>
        /// Initializes a new instance of the TabularQueryFormatter class
        /// </summary>
        internal TabularQueryFormatter()
        {
            Builder = new StringBuilder();
        }

        /// <summary>
        /// Gets or sets a StringBuilder that holds the query assembled so far 
        /// </summary>
        protected StringBuilder Builder { get; set; }

        /// <summary>
        /// Start the actual query assembly
        /// </summary>
        /// <param name="node">expression node to be formatted as query string</param>
        /// <returns>the query string</returns>
        internal string Format(Expression node)
        {
            this.Builder = new StringBuilder();
            var measures = new MeasureCollector().Collect(node).ToList();
            if (measures.Any(x => !string.IsNullOrEmpty(x.TableName)))
            {
                Builder.Append("DEFINE\n");
                foreach (var measureExpression in measures)
                {
                    var s = new DefineMeasuresBuilder().GetDefinition(measureExpression);
                    Builder.Append(s);
                }
            }
           
            this.Builder.Append("EVALUATE\n");
            Visit(node);
            return this.Builder.ToString();
        }

        /// <summary>
        /// Format "TOPN" expressions
        /// </summary>
        /// <param name="topnExpression">top expression</param>
        /// <returns>the same expression</returns>
        protected override Expression VisitTopn(TopnExpression topnExpression)
        {
            this.Builder.Append("TOPN(");
            this.Builder.Append(topnExpression.Top);
            this.Builder.Append(",");
            this.Visit(topnExpression.MainTable);
            this.Builder.Append(",");
            this.Visit(topnExpression.OrderBy);
            this.Builder.Append(")");
            return topnExpression;
        }

        /// <summary>
        /// Format Order By expressions within the top ordering context
        /// </summary>
        /// <param name="node">order by expression</param>
        /// <returns>the same expression</returns>
        protected override Expression VisitOrderBy(OrderByExpression node)
        {
            if (node.OrderColumn is MeasureExpression)
            {
                this.Builder.Append(node.OrderColumn.Name);
            }
            else
            {
                this.Builder.Append(node.OrderColumn.DbName);
            }

            this.Builder.Append(",");
            switch (node.OrderType)
            {
                case OrderType.Asc:
                    this.Builder.Append("1");
                    break;
                case OrderType.Desc:
                    this.Builder.Append("0");
                    break;
            }

            return node;
        }

        /// <summary>
        /// Generate query string part from aggregation expression
        /// </summary>
        /// <param name="aggregationExpression">input aggregation expression</param>
        /// <returns>the expression unchanged</returns>
        protected override Expression VisitXAggregation(XAggregationExpression aggregationExpression)
        {
            Builder.Append("\"" + aggregationExpression.Name.TrimStart('[').TrimEnd(']') + "\"");
            Builder.Append(",");
            var aggregationString = new XaggregationVisitor().GetAggragationString(aggregationExpression);
            this.Builder.Append(aggregationString);
            return aggregationExpression;
        }

        /// <summary>
        /// Generate Filter query part
        /// </summary>
        /// <param name="filterExpression">filter expression to be processed</param>
        /// <returns>the expression unchanged</returns>
        protected override Expression VisitFilter(FilterExpression filterExpression)
        {
            if (filterExpression.Filter == null)
            {
                return this.Visit(filterExpression.MainTable);
            }

            this.Builder.Append("FILTER(");
            this.Visit(filterExpression.MainTable);
            this.Builder.Append(",");
            this.Builder.Append(new FilterVisitor().GetFilterString(filterExpression.Filter));
            this.Builder.Append(")");
            return filterExpression;
        }

        /// <summary>
        /// Format generate expression
        /// </summary>
        /// <param name="generateExpression">the expression to be formatted as part of a query string</param>
        /// <returns>the expression unchanged</returns>
        protected override Expression VisitGenerate(GenerateExpression generateExpression)
        {
            var str = new GenerateVisitor().GetGenerateString(generateExpression);
            this.Builder.Append(str);
            return generateExpression;
        }

        /// <summary>
        /// Format summarize expression
        /// </summary>
        /// <param name="summarizeExpression">the expression to be formatted</param>
        /// <returns>the original expression</returns>
        protected override Expression VisitSummarizeExpression(SummarizeExpression summarizeExpression)
        {
            if (summarizeExpression.AllColumns != null && summarizeExpression.AllColumns.Any())
            {
                this.Builder.Append("\tSUMMARIZE(\n");
                this.Visit(summarizeExpression.MainTable);

                if (summarizeExpression.Columns.Any())
                {
                    IEnumerable<string> cs =
                        summarizeExpression.Columns.Select(c => ((ColumnExpression)c.Expression).DbName).Distinct();
                    string columnString = cs.Aggregate((c1, c2) => c1 + ",\n" + c2);
                    this.Builder.Append(",");
                    this.Builder.Append(columnString);
                }

                if (summarizeExpression.Measures.Any())
                {
                    this.VisitMeasures(summarizeExpression.Measures);
                    this.Builder.Append(")");
                }
                else
                {
                    this.Builder.Append(")");
                }
            }
            else
            {
                return this.Visit(summarizeExpression.MainTable);
            }

            return summarizeExpression;
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
        /// Format row expression
        /// </summary>
        /// <param name="node">row expression to be formatted</param>
        /// <returns>the original expression</returns>
        protected override Expression VisitRow(RowExpression node)
        {
            Builder.Append("ROW(");
            var m = (MeasureExpression)node.Measure.Expression;
            BuildMeasure(m);
            //this.Builder.Append("\"" + m.Name.TrimStart('[').TrimEnd(']') + "\"" + "," + "CALCULATE(" + m.DbName +
            //                     ",");
            //this.Visit(m.Filter);
            //this.Builder.Append(")");
            this.Builder.Append(")");
            return node;
        }

        /// <summary>
        /// Format values expression
        /// </summary>
        /// <param name="node">expression to be formatted</param>
        /// <returns>the original expression</returns>
        protected override Expression VisitValues(ValuesExpression node)
        {
            Builder.Append("VALUES(");
            Visit(node.Column.Expression);
            Builder.Append(")");
            return node;
        }

        /// <summary>
        /// Format All expression
        /// </summary>
        /// <param name="node">expression to be formatted</param>
        /// <returns>the original expression</returns>
        protected override Expression VisitAllExpression(AllExpression node)
        {
            if (node is AllSelectedExpression)
            {
                this.Builder.Append("ALLSELECTED(");
            }
            else
            {
                this.Builder.Append("ALL(");
            }

            this.Visit(node.Argument);
            this.Builder.Append(")");
            return node;
        }

        /// <summary>
        /// Format parameter expression 
        /// </summary>
        /// <param name="node">parameter expression</param>
        /// <returns>the original expression</returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            var t = node.Type.GetCustomAttribute(typeof(TabularTableMappingAttribute)) as TabularTableMappingAttribute;
            if (t != null)
            {
                this.Builder.Append(t.TableName);
                return base.VisitParameter(node);
            }

            return base.VisitParameter(node);
        }

        /// <summary>
        /// Format a table expression
        /// </summary>
        /// <param name="tableExpression">the table expression to be formatted</param>
        /// <returns>table expression unchanged</returns>
        protected override Expression VisitTable(TableExpression tableExpression)
        {
            this.Builder.Append(tableExpression.Name);
            return tableExpression;
        }

        /// <summary>
        /// Format a calculate table expression
        /// </summary>
        /// <param name="calculateTable">expression to be formatted</param>
        /// <returns>the original expression</returns>
        protected override Expression VisitCalculateTableExpression(CalculateTableExpression calculateTable)
        {
            if (calculateTable.Filters == null)
            {
                return this.Visit(calculateTable.MainTable);
            }

            this.Builder.Append("\tCALCULATETABLE(");
            this.Visit(calculateTable.MainTable);
            this.Builder.Append(",");
            this.Visit(calculateTable.Filters);
            this.Builder.Append(")");
            return calculateTable;
        }

        /// <summary>
        /// Format a column expression
        /// </summary>
        /// <param name="column">column expression</param>
        /// <returns>the original expression</returns>
        protected override Expression VisitColumn(ColumnExpression column)
        {
            var measureExpression = column as MeasureExpression;
            if (measureExpression != null)
            {
                if (column.DbName == null)
                {
                    return this.VisitMeasureColumn(measureExpression);
                }
            }

            this.Builder.Append(column.DbName);
            return column;
        }

        protected override Expression VisitMeasure(MeasureExpression measure)
        {
            
            return VisitColumn(measure);
        }

        /// <summary>
        /// Format Measure column
        /// </summary>
        /// <param name="column">the measure column expression</param>
        /// <returns>the original expression</returns>
        protected Expression VisitMeasureColumn(MeasureExpression column)
        {
            this.Builder.Append(column.Name);
            return column;
        }

        /// <summary>
        /// Format constant
        /// </summary>
        /// <param name="node">constant expression</param>
        /// <returns>original expression</returns>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value == null)
            {
                this.Builder.Append("BLANK()");
                return node;
            }

            switch (Type.GetTypeCode(node.Value.GetType()))
            {
                case TypeCode.DateTime:
                    this.Builder.Append("DATEVALUE(\"");
                    System.Threading.Thread.CurrentThread.CurrentCulture =
                        System.Globalization.CultureInfo.InvariantCulture;
                    this.Builder.Append(((DateTime)node.Value).ToShortDateString());
                    this.Builder.Append("\")");
                    break;
                case TypeCode.Boolean:
                    this.Builder.Append(node.Value);
                    break;
                case TypeCode.String:
                    this.Builder.Append("\"");
                    this.Builder.Append(node.Value);
                    this.Builder.Append("\"");
                    break;
                default:
                    this.Builder.Append(node.Value);
                    break;
            }

            return node;
        }

        /// <summary> Format Binary Expression </summary>
        /// <param name="node">binary expression to be formatted</param>
        /// <returns>the original expression unchanged</returns>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            this.Visit(node.Left);
            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                    this.Builder.Append(" = ");
                    break;
                case ExpressionType.NotEqual:
                    this.Builder.Append(" <> ");
                    break;
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    this.Builder.Append(" ,\n ");
                    break;
                case ExpressionType.LessThan:
                    this.Builder.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    this.Builder.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    this.Builder.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    this.Builder.Append(" >= ");
                    break;
                case ExpressionType.OrElse:
                    this.Builder.Append(" || ");
                    break;
                default:
                    throw new TabularException("this query is not supported yet, try to rephrase your query");
            }

            this.Visit(node.Right);

            return node;
        }

        /// <summary>
        /// Format Projection expression
        /// </summary>
        /// <param name="projectionExpression">input projection</param>
        /// <returns>original expression</returns>
        protected override Expression VisitProjection(ProjectionExpression projectionExpression)
        {
            return this.Visit(projectionExpression.Source);
        }

        /// <summary>
        /// Format unary expression
        /// </summary>
        /// <param name="node">input unary expression</param>
        /// <returns>the original expression</returns>
        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    this.Builder.Append("NOT(");
                    this.Visit(node.Operand);
                    this.Builder.Append(")");
                    break;

                    // case ExpressionType.Convert:
                    //    Visit(node.Operand);
                    //    break;
                default:
                    throw new NotImplementedException("Only not unary operator is supported yet");
            }
            return base.Visit(node);
        }

        /// <summary>
        /// Format a member expression 
        /// </summary>
        /// <param name="node">member node</param>
        /// <returns>the node unchanged</returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            if ((DaxExpressionType)node.Expression.NodeType == DaxExpressionType.Projection)
            {
                var mapping =
                    node.Member.GetCustomAttribute(typeof(TabularMappingAttribute)) as TabularMappingAttribute;
                if (mapping != null)
                {
                    string dbname = mapping.ColumnName;
                    string table = mapping.TableName;
                    var col = new ColumnExpression(node.Member.GetType(), node.Member.Name, dbname, table);
                    return this.Visit(col);
                }
            }

            return base.VisitMember(node);
        }

        /// <summary>
        /// Format UseRelationship.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitUseRelationship(UseRelationshipExpression node)
        {
            this.Builder.Append("USERELATIONSHIP(");
            this.Visit(node.Source);
            this.Builder.Append(",");
            this.Visit(node.Target);
            this.Builder.Append(")");
            return node;
        }

        /// <summary>
        /// Format measures 
        /// </summary>
        /// <param name="measures">collection of measures declarations</param>
        protected virtual void VisitMeasures(IEnumerable<ColumnDeclaration> measures)
        {
            IEnumerable<MeasureExpression> ms = measures.Select(c => ((MeasureExpression)c.Expression));

            foreach (MeasureExpression m in ms)
            {
                this.Builder.Append(",");
                BuildMeasure(m);
            }
        }

        private void BuildMeasure(MeasureExpression m)
        {
            if (string.IsNullOrEmpty(m.TableName))
            {
                FormatMeasure(m);
            }
            else
            {
                this.Builder.Append("\"" + m.Name.TrimStart('[').TrimEnd(']') + "\"" + ",");
                this.Builder.Append(m.TableName + m.Name);
            }
        }

        protected void FormatMeasure(MeasureExpression m)
        {
            if (m.Filter != null)
            {
                this.Builder.Append("\"" + m.Name.TrimStart('[').TrimEnd(']') + "\"" + "," + "CALCULATE(" +
                                    m.DbName +
                                    ",");
                this.Visit(m.Filter);
                this.Builder.Append(")");
            }
            else if (m is LookupExpression)
            {
                this.Builder.Append("\"" + m.Name.TrimStart('[').TrimEnd(']') + "\"" + "," + m.DbName);
            }
            else if (m is XAggregationExpression)
            {
                this.Visit(m as XAggregationExpression);
            }
            else if (m.DbName != null)
            {
                this.Builder.Append("\"" + m.Name.TrimStart('[').TrimEnd(']') + "\"" + "," + "CALCULATE(" +
                                    m.DbName +
                                    ")");
            }
            else
            {
                this.Builder.Append(m.Name);
            }
        }
    }
}