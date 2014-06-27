namespace LinqToDAX.Query
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;
    using System.Reflection;
    using LinqToDAX.Query.DAXExpression;

    /// <summary>
    /// Abstract base class to use for reflected methods in query execution 
    /// </summary>
    public abstract class ProjectionRow
    {
        /// <summary>
        /// Gets an individual result from the result reader
        /// </summary>
        /// <param name="index">string reference for the column in the reader</param>
        /// <param name="t">type of the column</param>
        /// <returns>value of the column in the row</returns>
        public abstract object GetValue(string index, Type t);

        /// <summary>
        /// Executes a sub query independently
        /// </summary>
        /// <param name="query">query expression</param>
        /// <typeparam name="T">result type</typeparam>
        /// <returns>result object</returns>
        public abstract IEnumerable<T> ExecuteSubQuery<T>(LambdaExpression query);
    }

    /// <summary>
    /// Class responsible for generating get values for columns
    /// </summary>
    internal class ColumnProjector : DaxExpressionVisitor
    {
        private static MethodInfo _getValue;
        private readonly Dictionary<ColumnExpression, ColumnExpression> _map;
        private readonly Nominator _nominator;
        private HashSet<Expression> _candidates;
        private List<ColumnDeclaration> _columns;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnProjector"/> class.
        /// </summary>
        /// <param name="canBeColumn">Function that can decide if the expression is a column reference</param>
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
       
        /// <summary>
        /// Visitor function for column projection
        /// </summary>
        /// <param name="expression">expression to be visited</param>
        /// <returns>the expression</returns>
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

        /// <summary>
        /// Executes the column projection for the given node, by visiting the expression
        /// </summary>
        /// <param name="node">expression from which we extract columns</param>
        /// <returns>collection of resolved column projections</returns>
        internal ProjectedColumns ProjectColumns(Expression node)
        {
            _columns = new List<ColumnDeclaration>();
            _candidates = _nominator.Nominate(node);
            Expression e = Visit(node);
            List<ColumnDeclaration> cs = _columns;
            return new ProjectedColumns(e, cs.AsReadOnly());
        }
    }

    /// <summary>
    /// Result class of projection
    /// </summary>
    internal sealed class ProjectedColumns
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectedColumns"/> class.
        /// </summary>
        /// <param name="projector">Expression with projections</param>
        /// <param name="columns">Column collection</param>
        internal ProjectedColumns(Expression projector, ReadOnlyCollection<ColumnDeclaration> columns)
        {
            Projector = projector;
            Columns = columns;
        }

        /// <summary>
        /// Gets the projected column collection
        /// </summary>
        public ReadOnlyCollection<ColumnDeclaration> Columns { get; private set; }
        
        /// <summary>
        /// Gets the projector expression
        /// </summary>
        public Expression Projector { get; private set; }
    }
}