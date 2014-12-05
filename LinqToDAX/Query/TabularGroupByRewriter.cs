using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LinqToDAX.Query.DAXExpression;

namespace LinqToDAX.Query
{
    internal class TabularGroupByRewriter 
    {
        internal GroupingProjection Grouping { get; private set; }
        internal TabularGroupByRewriter(GroupingProjection groupByQuery)
        {
            this.Grouping = groupByQuery;
        }

        internal Expression Rewrite()
        {
            var keys = Grouping.KeyProjection;
            var keyColumns = new ColumnProjector(TabularExpressionHelper.CanBeColumn).ProjectColumns(keys);
            var keyFilters = keyColumns.Columns.Select(c => Expression.Equal(c.Expression, c.Expression)).Aggregate((x,y) => Expression.And(x,y));

            var subQueryBase = 
                new CalculateTableExpression(Grouping.SubQueryProjectionExpression.Type,
                    Grouping.SubQueryProjectionExpression.Source, keyFilters);
            var subQuery = new SubQueryProjection(Grouping.SubQueryProjectionExpression.Type, new ProjectionExpression(subQueryBase, Grouping.SubQueryProjectionExpression.Projector));
            Type groupType =
                typeof (TabularGrouping<,>).MakeGenericType(new Type[]
                {Grouping.KeyProjection.Type, Grouping.SubQueryProjectionExpression.Type});
            return new ProjectionExpression(new SummarizeExpression(groupType, new List<ColumnDeclaration>().AsReadOnly(), Grouping.SubQueryProjectionExpression.Source), subQuery);
        }
    }
}
