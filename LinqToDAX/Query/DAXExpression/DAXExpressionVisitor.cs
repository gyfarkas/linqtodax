// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DAXExpressionVisitor.cs" company="Dealogic">
//  see LICENCE.md
// </copyright>
// <summary>
//   Defines the DaxExpressionVisitor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LinqToDAX.Query.DAXExpression
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;

    internal class DaxExpressionVisitor : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return null;
            }

            switch ((DaxExpressionType)node.NodeType)
            {
                case DaxExpressionType.Projection:
                    return VisitProjection((ProjectionExpression)node);
                case DaxExpressionType.AddColumns:
                    return VisitAddColumns((AddColumnsExpression)node);
                case DaxExpressionType.CalculateTable:
                    return VisitCalculateTableExpression((CalculateTableExpression)node);
                case DaxExpressionType.Summarize:
                    return VisitSummarizeExpression((SummarizeExpression)node);
                case DaxExpressionType.Topn:
                    return VisitTopn((TopnExpression)node);
                case DaxExpressionType.Table:
                    return VisitTable((TableExpression)node);
                case DaxExpressionType.Column:
                    return VisitColumn((ColumnExpression)node);
                case DaxExpressionType.Measure:
                    return VisitMeasure((MeasureExpression)node);
                case DaxExpressionType.OrderByItem:
                    return VisitOrderBy((OrderByExpression)node);
                case DaxExpressionType.Filter:
                    return VisitFilter((FilterExpression)node);
                case DaxExpressionType.All:
                    return VisitAllExpression((AllExpression)node);
                case DaxExpressionType.Lookup:
                    return VisitLookup((LookupExpression)node);
                case DaxExpressionType.XAggregation:
                    return VisitXAggregation((XAggregationExpression)node);
                case DaxExpressionType.Generate:
                    return VisitGenerate((GenerateExpression)node);
                case DaxExpressionType.Row:
                    return VisitRow((RowExpression)node);
                case DaxExpressionType.Values:
                    return VisitValues((ValuesExpression)node);
                case DaxExpressionType.UseRelationship:
                    return VisitUseRelationship((UseRelationshipExpression)node);
            }
            
            return base.Visit(node);
        }

        protected virtual Expression VisitUseRelationship(UseRelationshipExpression node)
        {
            var source = (ColumnExpression) Visit(node.Source);
            var target = (ColumnExpression) Visit(node.Target);
            if (source != node.Source || target != node.Target)
            {
                return new UseRelationshipExpression(source, target);
            }
            return node;
        }

        protected virtual Expression VisitValues(ValuesExpression node)
        {
            var col = Visit(node.Column.Expression);
            if (col != node.Column.Expression)
            {
                return new ValuesExpression(col.Type, new ColumnDeclaration(node.Column.Name, node.Column.Alias, node.Column.Expression));
            }
            return node;
        }

        protected virtual Expression VisitRow(RowExpression node)
        {
            var measure = Visit(node.Measure.Expression);
            if (node.Measure.Expression != measure)
            {
                return new RowExpression(measure.Type, new MeasureDeclaration(node.Measure.Name, node.Measure.Alias, node.Measure.Expression));
            }

            return node;
        }

        protected virtual Expression VisitGenerate(GenerateExpression generateExpression)
        {
            var source = Visit(generateExpression.Source);
            var gen = Visit(generateExpression.Generator);
            if (source != generateExpression.Source || gen != generateExpression.Generator)
            {
                return new GenerateExpression(generateExpression.Type, source,gen);
            }

            return generateExpression;
        }

        protected virtual Expression VisitMeasure(MeasureExpression measure)
        {
            return measure;
        }

        protected virtual Expression VisitXAggregation(XAggregationExpression aggregationExpression)
        {
            //var source = Visit(aggregationExpression.Source);
            //var filter = Visit(aggregationExpression.Filter);
            //var column = Visit(aggregationExpression.Column) as ColumnExpression;
            //if (column == null)
            //{
            //    throw new TabularException(string.Format("Invalid Column in aggregation: {0}", aggregationExpression.Column.Name));
            //}

            //if (source != aggregationExpression.Source || filter != aggregationExpression.Filter ||
            //    column != aggregationExpression.Column)
            //{
            //   return new XAggregationExpression(aggregationExpression.AggregationType, source, column, aggregationExpression.Name, aggregationExpression.Type, filter);
            //}

            return aggregationExpression;
        }

        protected virtual Expression VisitLookup(LookupExpression lookupExpression)
        {
            return lookupExpression;
        }

        protected virtual Expression VisitAllExpression(AllExpression node)
        {
            Expression arg = Visit(node.Argument);
            if (arg == node.Argument) return node;
            if (node is AllSelectedExpression)
            {
                return new AllSelectedExpression(node.Type, arg);
            }
            return new AllExpression(node.Type, arg);
        }


        protected virtual Expression VisitOrderBy(OrderByExpression node)
        {
            return node;
        }

        protected virtual Expression VisitAddColumns(AddColumnsExpression addColumnsExpression)
        {
            Expression table = VisitSource(addColumnsExpression.MainTable);
            ReadOnlyCollection<ColumnDeclaration> columns =
                VisitColumnDeclarations(addColumnsExpression.Columns);
            if (table != addColumnsExpression.MainTable || columns != addColumnsExpression.Columns)
            {
                return new AddColumnsExpression(addColumnsExpression.Type, columns, table);
            }

            return addColumnsExpression;
        }

        protected virtual Expression VisitFilter(FilterExpression filterExpression)
        {
            Expression table = VisitSource(filterExpression.MainTable);
            Expression where = Visit(filterExpression.Filter);
            if (table != filterExpression.MainTable || where != filterExpression.Filter)
            {
                return new FilterExpression(filterExpression.Type, table, where);
            }

            return filterExpression;
        }

        protected virtual Expression VisitTopn(TopnExpression topnExpression)
        {
            Expression table = VisitSource(topnExpression.MainTable);
            if (table != topnExpression.MainTable)
            {
                return new TopnExpression(topnExpression.Type, table, topnExpression.OrderBy, topnExpression.Top);
            }

            return topnExpression;
        }

        protected virtual Expression VisitCalculateTableExpression(CalculateTableExpression calculateTableExpression)
        {
            Expression table = VisitSource(calculateTableExpression.MainTable);
            Expression where = Visit(calculateTableExpression.Filters);
            if (table != calculateTableExpression.MainTable || where != calculateTableExpression.Filters)
            {
                return new CalculateTableExpression(calculateTableExpression.Type,
                    table, where);
            }
            return calculateTableExpression;
        }

        protected virtual Expression VisitColumn(ColumnExpression columnExpression)
        {
            return columnExpression;
        }

        protected virtual Expression VisitTable(TableExpression tableExpression)
        {
            return tableExpression;
        }

        protected virtual Expression VisitSummarizeExpression(SummarizeExpression summarizeExpression)
        {
            Expression table = VisitSource(summarizeExpression.MainTable);
            ReadOnlyCollection<ColumnDeclaration> columns =
                VisitColumnDeclarations(summarizeExpression.AllColumns);
            if (table != summarizeExpression.MainTable || columns != summarizeExpression.AllColumns)
            {
                return new SummarizeExpression(summarizeExpression.Type, columns, table);
            }

            return summarizeExpression;
        }

        protected virtual ReadOnlyCollection<ColumnDeclaration>
            VisitColumnDeclarations(ReadOnlyCollection<ColumnDeclaration> readOnlyCollection)
        {
            
            var alternate = readOnlyCollection.Select(VisitColumnDeclaration);
            return alternate.ToList().AsReadOnly();

        }

        protected virtual ColumnDeclaration VisitColumnDeclaration(ColumnDeclaration columnDeclaration)
        {
            var alternarte = Visit(columnDeclaration.Expression);
            if (alternarte != columnDeclaration.Expression)
            {
                return new ColumnDeclaration(columnDeclaration.Name, columnDeclaration.Alias, alternarte);
            }

            return columnDeclaration;
        }

        protected virtual Expression VisitProjection(ProjectionExpression projectionExpression)
        {
            var source = (DaxExpression)Visit(projectionExpression.Source);
            if ((DaxExpressionType)source.NodeType == DaxExpressionType.Projection)
            {
                Visit(((ProjectionExpression)source).Source);
            }

            Expression projector = Visit(projectionExpression.Projector);
            if (source != projectionExpression.Source || projector != projectionExpression.Projector)
            {
                return new ProjectionExpression(source, projector);
            }

            return projectionExpression;
        }

        protected virtual Expression VisitSource(Expression source)
        {
            return Visit(source);
        }
    }
}