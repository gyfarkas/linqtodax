using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using LinqToDAX.Query.DAXExpression;

namespace LinqToDAX.Query
{
    public abstract class ProjectionRow
    {
        public abstract object GetValue(string index, Type t);
    }


    internal class ColumnProjector : DaxExpressionVisitor
    {
        private static MethodInfo _getValue;
        private readonly Dictionary<ColumnExpression, ColumnExpression> _map;
        private readonly Nominator _nominator;
        private HashSet<Expression> _candidates;
        private List<ColumnDeclaration> _columns;

        internal ColumnProjector(Func<Expression, bool> canBeColumn)
        {
            if (_getValue == null)
            {
                _getValue = typeof(ProjectionRow).GetMethod("GetValue");
            }

            _nominator = new Nominator(canBeColumn);
            _candidates = new HashSet<Expression>();

            _map = new Dictionary<ColumnExpression, ColumnExpression>();
        }

        internal ProjectedColumns ProjectColumns(Expression node)
        {
            _columns = new List<ColumnDeclaration>();
            _candidates = _nominator.Nominate(node);
            Expression e = Visit(node);
            List<ColumnDeclaration> cs = _columns;
            return new ProjectedColumns(e, cs.AsReadOnly());
        }

        public override Expression Visit(Expression expression)
        {
            if (_candidates.Contains(expression))
            {
                var measureExpression = expression as MeasureExpression;
                if (measureExpression != null)
                {
                    MeasureExpression m = measureExpression;
                    ColumnExpression mappedMeasure;
                    if (_map.TryGetValue(m, out mappedMeasure))
                    {
                        return mappedMeasure;
                    }

                    _columns.Add(new MeasureDeclaration(m.Name, m.Name, m));

                    // The mapped measure can only be used as backreference to something known
                    mappedMeasure = new MeasureExpression(m.Type, m.Name, null, m.Name, null);
                    _map[m] = mappedMeasure;
                    return mappedMeasure;
                }

                var column = expression as ColumnExpression;
                if (column == null)
                {
                    return base.Visit(expression);
                }

                ColumnExpression mapped;
                if (_map.TryGetValue(column, out mapped))
                {
                    return mapped;
                }

                _columns.Add(new ColumnDeclaration(column.Name, column.Name, column));
                mapped = new ColumnExpression(column.Type, column.Name, column.DbName, column.TableName);
                _map[column] = mapped;
                return mapped;
            }

            return base.Visit(expression);
        }
    }

    internal sealed class ProjectedColumns
    {
        internal ProjectedColumns(Expression projector, ReadOnlyCollection<ColumnDeclaration> columns)
        {
            Projector = projector;
            Columns = columns;
        }

        public ReadOnlyCollection<ColumnDeclaration> Columns { get; private set; }
        public Expression Projector { get; private set; }
    }
}