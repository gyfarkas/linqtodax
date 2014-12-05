// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TabularQueryBinder.cs" company="Dealogic">
//   see LICENCE.md
// </copyright>
// <summary>
//   Defines the TabularQueryBinder type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.CodeDom;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace LinqToDAX.Query
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using DAXExpression;
    using TabularEntities;
    
    /// <summary>
    /// Class responsible for transforming LINQ Expression trees to DAX expression trees, 
    /// with ProjectionExpression as root
    /// </summary>
    internal class TabularQueryBinder : ExpressionVisitor
    {
        /// <summary>
        /// Component that creates AllExpressions 
        /// </summary>
        private readonly AllExpressionFactory _allExpressionFactory;

        /// <summary>
        /// Component that creates column expressions
        /// </summary>
        private readonly ColumnExpressionFactory _columnExpressionFactory;

        /// <summary>
        /// Component that creates table expressions
        /// </summary>
        private readonly TableFactory _tableFactory;

        /// <summary>
        /// Component that finds and allocates columns in expressions
        /// </summary>
        private readonly ColumnProjector _columnProjector;

        private TabularQueryProvider Provider { get; set; }

        /// <summary>
        /// Map that keeps track of parameters and their reference
        /// </summary>
        private Dictionary<ParameterExpression, Expression> _parameterMap;

        private List<Tuple<Expression, OrderType>> _orderBys;

        internal ReadOnlyCollection<Tuple<Expression, OrderType>> OrderByColumns
        {
            get
            {
                return new ReadOnlyCollection<Tuple<Expression, OrderType>>(this._orderBys);
            } 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TabularQueryBinder"/> class. 
        /// </summary>
        internal TabularQueryBinder(TabularQueryProvider provider)
        {
            _columnProjector = new ColumnProjector(TabularExpressionHelper.CanBeColumn);
            _columnExpressionFactory = new ColumnExpressionFactory(this);
            _tableFactory = new TableFactory(this);
            _allExpressionFactory = new AllExpressionFactory(this);
            _parameterMap = new Dictionary<ParameterExpression, Expression>();
            _orderBys = new List<Tuple<Expression, OrderType>>();
            Provider = provider;
        }

        /// <summary>
       /// Visits the expression passed and replaces sub-expressions, 
       /// and creates a projection expression in the end. 
       /// with DAX specific parts while collecting information as well for further processing
       /// </summary>
       /// <param name="node">the original expression to transform</param>
       /// <returns>the transformed expression, ProjectionExpression</returns>
       internal Expression Bind(Expression node)
        {
            if (node is DaxExpression)
            {
                return node;
            }

            _parameterMap = new Dictionary<ParameterExpression, Expression>();

            return Visit(node);
        }

        internal Expression BindRelative(Expression node, Dictionary<ParameterExpression, Expression> d)
        {
            if (node is DaxExpression)
            {
                return node;
            }

            _parameterMap = d;

            return Visit(node);
        }
        
        /// <summary>
        /// Calls the ColumnProjector to find and match columns 
        /// </summary>
        /// <param name="node">node that supposed to have columns</param>
        /// <returns>ProjectedColumns that contains the columns found as well as the source</returns>
        internal ProjectedColumns ProjectColumns(Expression node)
        {
            return _columnProjector.ProjectColumns(node);
        }

        /// <summary>
        /// Binds generate function call (called by the Generate extension method)
        /// </summary>
        /// <param name="source">table expression serving as basis</param>
        /// <param name="generator">table expression that generates the additional columns in the result</param>
        /// <param name="selector">function (lambda) that projects the result</param>
        /// <returns>projection expression enclosing a new tabular table created from the source 
        /// and the generator and projected by the projector function</returns>
        internal Expression BindGenerate(Expression source, Expression generator, LambdaExpression selector)
        {
            var sourceProjection = (source is ProjectionExpression) ? (ProjectionExpression)source : (ProjectionExpression)Visit(source);
            var generatorProjection = (ProjectionExpression)Visit(generator);
            _parameterMap[selector.Parameters[0]] = sourceProjection.Projector;
            _parameterMap[selector.Parameters[1]] = generatorProjection.Projector;
            var body = Visit(selector.Body);
            var pc = ProjectColumns(body);
            Type elevatedType = typeof(TabularTable<>).MakeGenericType(pc.Projector.Type);
            var generateExpression = new GenerateExpression(body.Type, sourceProjection.Source, generatorProjection.Source);
            return new ProjectionExpression(elevatedType, generateExpression, pc.Projector);
        }

        /// <summary>
        /// Transformation of method calls in expressions
        /// </summary>
        /// <param name="node">a method call to be translated</param>
        /// <returns>translated expression</returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable) || node.Method.DeclaringType == typeof(Enumerable))
            {
                switch (node.Method.Name)
                {
                    case "Where":
                        return BindWhere(node.Type, node.Arguments[0], (LambdaExpression)TabularExpressionHelper.StripQuotes(node.Arguments[1]));
                    case "Select":
                        return BindSelect(node.Type, node.Arguments[0], (LambdaExpression)TabularExpressionHelper.StripQuotes(node.Arguments[1]));
                    case "Take":
                        return BindTake(node.Type, node.Arguments[0], (ConstantExpression)node.Arguments[1]);
                    case "SelectMany":
                        if (node.Arguments.Count == 1)
                        {
                            return BindSelectMany(node.Type, node.Arguments[0],null, null);
                        }


                        if (node.Arguments.Count == 2)
                        {
                            return BindSelectMany(node.Type, node.Arguments[0],
                                (LambdaExpression)TabularExpressionHelper.StripQuotes(node.Arguments[1]),null);
                        }

                        if (node.Arguments.Count == 3)
                        {
                            return BindSelectMany(node.Type, node.Arguments[0],
                                (LambdaExpression) TabularExpressionHelper.StripQuotes(node.Arguments[1]),
                                (LambdaExpression) TabularExpressionHelper.StripQuotes(node.Arguments[2]));
                        }
                        break;
                    case "GroupBy":
                        if (node.Arguments.Count == 2)
                        {
                            return this.BindGroupBy(node.Type, node.Arguments[0], (LambdaExpression) TabularExpressionHelper.StripQuotes(node.Arguments[1]), null, null);
                        }
                        else if (node.Arguments.Count == 3)
                        {
                            var lambda1 = (LambdaExpression) TabularExpressionHelper.StripQuotes(node.Arguments[1]);
                            var lambda2 = (LambdaExpression) TabularExpressionHelper.StripQuotes(node.Arguments[2]);
                            if (lambda2.Parameters.Count == 1)
                            {
                                // second lambda is element selector
                                return this.BindGroupBy(node.Type, node.Arguments[0], lambda1, lambda2, null);
                            }
                            else if (lambda2.Parameters.Count == 2)
                            {
                                // second lambda is result selector
                                return this.BindGroupBy(node.Type, node.Arguments[0], lambda1, null, lambda2);
                            }
                        }
                        else if (node.Arguments.Count == 4)
                        {
                            return this.BindGroupBy(node.Type, node.Arguments[0], (LambdaExpression)TabularExpressionHelper.StripQuotes(node.Arguments[1]), (LambdaExpression)TabularExpressionHelper.StripQuotes(node.Arguments[2]), (LambdaExpression) TabularExpressionHelper.StripQuotes(node.Arguments[3]));
                        }
                        break;
                    case "Join":
                        throw new NotImplementedException();
                    case "GroupJoin":
                        throw new NotImplementedException();
                    case "OrderBy":
                        return BindOrderBy(node.Type, node.Arguments[0], (LambdaExpression)TabularExpressionHelper.StripQuotes(node.Arguments[1]),OrderType.Asc);
                    case "OrderByDescending":
                        return BindOrderBy(node.Type, node.Arguments[0], (LambdaExpression)TabularExpressionHelper.StripQuotes(node.Arguments[1]),OrderType.Desc);
                    case "ThenBy":
                        return BindOrderBy(node.Type, node.Arguments[0], (LambdaExpression)TabularExpressionHelper.StripQuotes(node.Arguments[1]),OrderType.Asc);
                    case "ThenByDescending":
                        return BindOrderBy(node.Type, node.Arguments[0], (LambdaExpression)TabularExpressionHelper.StripQuotes(node.Arguments[1]), OrderType.Desc);
                    case "Sum":
                        return BindAggregation(node, AggregationType.Sum);
                    case "Count":
                        throw new TabularException("int Count is not supported use LongCount or tabular defined countx, distinctcount or countrows");
                        //return BindAggregation(node, AggregationType.Count);
                    case "LongCount":
                        return BindAggregation(node, AggregationType.Count);
                    case "Average":
                        return BindAggregation(node, AggregationType.Avg);
                    case "Min":
                        return BindAggregation(node, AggregationType.Min);
                    case "Max":
                        return BindAggregation(node, AggregationType.Max);
                    // Distinct is always implicit in selects so we skip it
                    case "Distinct" :
                        return Visit(node.Arguments[0]);
                    default:
                        throw new NotImplementedException(string.Format("no method to deal with : {0}", node.Method.Name));
                }
            }

            if (node.Method.DeclaringType == typeof(IQueryable))
            {
                return base.VisitMethodCall(node);
            }

            return TabularFunction(node);
        }

        private Expression BindOrderBy(Type type, Expression expression, LambdaExpression lambdaExpression, OrderType orderType)
        {
            var source = Visit(expression);
            var projectionExpression = source as ProjectionExpression;
            if (projectionExpression != null)
            {
                _parameterMap[lambdaExpression.Parameters[0]] = projectionExpression.Projector ;
            }
            else    
            {
                _parameterMap[lambdaExpression.Parameters[0]] = source;
            }
            
            var os = Visit(lambdaExpression.Body);
            
            _orderBys.Add(Tuple.Create(os,orderType));
            
            
            return source;
        }

        /// <summary>
        /// Transformation of constants
        /// </summary>
        /// <param name="node">constant expression</param>
        /// <returns>transformed construct</returns>
        protected override Expression VisitConstant(ConstantExpression node)
        {
           
            if (node.Type.UnderlyingSystemType.Name == "DaxProviderTable" 
                || node.Type.UnderlyingSystemType.Name == typeof(TabularTable<>).Name 
                || (node.Value != null && node.Value.GetType().Name == typeof(TabularTable<>).Name)
                )
            {
                var queryable = node.Value as IQueryable;
                if (queryable != null)
                {
                    ProjectionExpression proj = _tableFactory.GetTableProjection(queryable);
                    if (proj != null)
                    {
                        return proj;
                    }
                }
                else
                {
                    throw new TabularException("IQueryable expected in table Projection");
                }
            }

            return node;
        }

        /// <summary>
        /// Transforms parameter expressions according to their references found in the expression
        /// </summary>
        /// <param name="node">parameter expression to be substituted</param>
        /// <returns>substitutable reference if found</returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            Expression exp;
            if (_parameterMap.TryGetValue(node, out exp))
            {
                return exp;
            }

            return node;
        }

        /// <summary>
        /// Replaces conditional expressions when the test is evaluated to a boolean constant.
        /// </summary>
        /// <param name="node">conditional expression</param>
        /// <returns>one of the branches if the test is evaluated or the original conditional expression, 
        /// no DAX translation happens at the moment</returns>
        protected override Expression VisitConditional(ConditionalExpression node)
        {
            if (node.Test.NodeType == ExpressionType.Constant)
            {
                if ((bool)((ConstantExpression)node.Test).Value)
                {
                    return Visit(node.IfTrue);
                }

                return Visit(node.IfFalse);
            }

            return base.VisitConditional(node);
        }

        /// <summary>
        /// simply calls the member access has only compatibility role 
        /// </summary>
        /// <param name="node">member expression</param>
        /// <returns>transformed expression </returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            return VisitMemberAccess(node);
        }

        /// <summary>
        /// Member access binder
        /// </summary>
        /// <param name="m">member access expression</param>
        /// <returns>Expression column corresponding to the member</returns>
        protected Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression.NodeType == ExpressionType.Parameter)
            {
                // We are referring back to something we created
                Expression exp;
                if (_parameterMap.TryGetValue((ParameterExpression)m.Expression, out exp))
                {
                    if (m.Expression.NodeType == ExpressionType.Parameter)
                    {
                        var property = m.Member as PropertyInfo;
                        var ex = FindMember(exp, property);
                        return ex;
                    }

                    return _columnExpressionFactory.FindColumnExpression(exp, m.Member);
                }

                // Referring to a member
                if (typeof(ITabularData).IsAssignableFrom(m.Expression.Type))
                {
                    return _columnExpressionFactory.CreateColumnExpression(m);
                }
              
                throw new TabularException("Referred to a parameter not mapped, that should not have happened");
            }

            // Referring to a related table member
            if (typeof(ITabularData).IsAssignableFrom(m.Type) && m.Expression.NodeType == ExpressionType.MemberAccess)
            {
                Expression mem = FindParameter(m.Member.DeclaringType);
                Expression result = FindMember(mem, (PropertyInfo)m.Member);
                if (result != null)
                {
                    return _columnExpressionFactory.FindColumnExpression(result, m.Member);
                }
            }

            return _columnExpressionFactory.CreateColumnExpression(m);
        }

        /// <summary>
        /// Captures subordinate queries
        /// </summary>
        /// <param name="node">node to be bound</param>
        /// <returns>new expression with subordinate queries transformed</returns>
        protected override Expression VisitNew(NewExpression node)
        {
            var args = node.Arguments.Select(Visit).ToArray();
            if (args.Any(a => 
                (DaxExpressionType)a.NodeType == DaxExpressionType.Projection 
                || (DaxExpressionType)a.NodeType == DaxExpressionType.SubQuery
                || (DaxExpressionType)a.NodeType == DaxExpressionType.Grouping
                || (DaxExpressionType)a.NodeType == DaxExpressionType.Table))
            {
                var changed = false;
                for (int i = 0; i < args.Length; i++)
                {
                    var p = args[i] as ProjectionExpression;
                    var g = args[i] as GroupingProjection;

                    if ((p != null && p.Source == null) || args[i] is SubQueryProjection)
                    {
                        changed = true;
                        continue;
                    }

                    if (p != null && (DaxExpressionType)p.Source.NodeType != DaxExpressionType.Table)
                    {
                        var type = typeof(IQueryable<>).MakeGenericType(args[i].Type);
                        var proj = (ProjectionExpression)args[i];
                        args[i] = new SubQueryProjection(type, proj); 
                        changed = true;
                    }

                    if (g != null)
                    {
                        throw new TabularException("Grouping cannot be selected");
                    }
                   
                }
                
                if (changed)
                {
                    return Expression.New(node.Constructor, args);
                }
            }
           

            return base.VisitNew(node);
        }



        /// <summary>
        /// Selects the appropriate method to deal with functions, methods defined as extensions
        /// </summary>
        /// <param name="node">method call</param>
        /// <returns>measure expression generated by the function call</returns>
        private Expression TabularFunction(MethodCallExpression node)
        {
            if (node.Method.DeclaringType != typeof(TabularQueryExtensions))
            {
                return CreateMappedMeasureExpression(node);
            }

            switch (node.Method.Name)
            {
                case "Sum":
                case "Max":
                case "Min":
                case "Average":
                case "DistinctCount":
                    return _columnExpressionFactory.CreateMeasureExpression(node);

                case "Sumx":
                    return _columnExpressionFactory.CreateXAggregation(AggregationType.Sum, node);
                case "Rankx":
                    return _columnExpressionFactory.CreateXAggregation(AggregationType.Rank, node);
                case "ReverseRankx":
                    return _columnExpressionFactory.CreateXAggregation(AggregationType.ReverseRank, node);
                case "Maxx":
                    return _columnExpressionFactory.CreateXAggregation(AggregationType.Max, node);
                case "Minx":
                    return _columnExpressionFactory.CreateXAggregation(AggregationType.Min, node);
                case "Countx":
                    return _columnExpressionFactory.CreateXAggregation(AggregationType.Count, node);
                case "Averagex":
                    return _columnExpressionFactory.CreateXAggregation(AggregationType.Avg, node);

                case "ForAll":
                    return _allExpressionFactory.CreateAllExpression(node, AllType.All);
                case "ForAllSelected":
                    return _allExpressionFactory.CreateAllExpression(node, AllType.AllSelected);
                case "LookupValue":
                    return _columnExpressionFactory.CreateLookupExpression(node);
                case "Generate":
                    return BindGenerate(node.Arguments[0], node.Arguments[1], (LambdaExpression)node.Arguments[2]);
                case "UseRelationship":
                    var arg1 = (ColumnExpression)Visit(node.Arguments[0]);
                    var arg2 = (ColumnExpression)Visit(node.Arguments[1]);
                    return new UseRelationshipExpression(arg1, arg2);
                case "ApplyRelationship":
                    var columnexp = (ColumnExpression)Visit(node.Arguments[0]);
                    var columnname = ((ConstantExpression)node.Arguments[1]).Value as string;
                    return new UseRelationshipExpression(columnexp, new ColumnExpression(columnexp.Type, columnexp.NodeType, columnname, columnname, string.Empty));
                case "CountRows":
                    return _columnExpressionFactory.CreateCountRows(AggregationType.CountRows, node);
            }

            return CreateMappedMeasureExpression(node);
        }

        /// <summary>
        /// Maps method calls to measures
        /// </summary>
        /// <param name="node">method call</param>
        /// <returns>measure expression</returns>
        private Expression CreateMappedMeasureExpression(MethodCallExpression node)
        {
            var attribute =
                node.Method.GetCustomAttribute(typeof(TabularMeasureMappingAttribute)) as
                    TabularMeasureMappingAttribute;

            int minArgCount = node.Method.IsStatic ? 1 : 0;

            if (attribute != null && node.Arguments.Count() == minArgCount)
            {
                Type type = node.Method.ReturnType;
                string dbname = attribute.MeasureName;
                string name = attribute.MeasureName;
                var mes = new MeasureExpression(type, name, dbname, name, string.Empty);
                return mes;
            }

            if (attribute != null && node.Arguments.Count() == minArgCount + 1)
            {
                Type type = node.Method.ReturnType;
                string dbname = attribute.MeasureName;
                string name = attribute.MeasureName;
                Expression filter = base.Visit(node.Arguments[minArgCount]);
                var mes = new MeasureExpression(type, (ExpressionType)DaxExpressionType.Measure, name, dbname, name, string.Empty, filter);
                return mes;
            }

            return base.VisitMethodCall(node);
        }

        private Expression BindAggregation(Expression node, AggregationType aggregationType)
        {
            var method = (MethodCallExpression)node;
            if (method.Arguments.Count() == 1)
            {
                var p = Visit(method.Arguments[0]);
                ProjectionExpression proj = null;
                if (p is ProjectionExpression)
                {
                    proj = (ProjectionExpression) p;
                }
                if (p is GroupingProjection)
                {
                    proj = ((GroupingProjection) p).KeyProjection;
                }
                if (proj == null)
                {
                    throw new TabularException("Could not find projection for Sum");
                }
                var column = proj.Projector as ColumnExpression;
                    if (column != null)
                    {
                        string aggregationString;
                        switch (aggregationType)
                        {
                            case AggregationType.Sum:
                                aggregationString = string.Format("SUM({0})", column.DbName);
                                break;
                            case AggregationType.Max:
                                aggregationString = string.Format("MAX({0})", column.DbName);
                                break;
                            case AggregationType.Avg:
                                aggregationString = string.Format("AVERAGE({0})", column.DbName);
                                break;
                            case AggregationType.Min:
                                aggregationString = string.Format("MIN({0})", column.DbName);
                                break;
                            case AggregationType.Count:
                            case AggregationType.LongCount:
                                 aggregationString = string.Format("DISTINCTCOUNT({0})", column.DbName);
                                break;
                            default: 
                                throw new NotImplementedException(String.Format("not implemented aggregation detected: {0}", aggregationType));
                        }
                        
                        if ((DaxExpressionType)proj.Source.NodeType == DaxExpressionType.CalculateTable)
                        {
                            return new MeasureExpression(
                                method.Method.ReturnType, 
                                (ExpressionType)DaxExpressionType.Measure, 
                                "[AggregateOf" + column.Name + "]", 
                                aggregationString, 
                                "[AggregateOf" + column.Name + "]", 
                                column.TableName, 
                                ((CalculateTableExpression)proj.Source).Filters);
                        }

                        return new MeasureExpression(method.Method.ReturnType, "[AggregateOf" + column.Name + "]", aggregationString, "[AggregateOf" + column.Name + "]", column.TableName);
                    }
                
                return _columnExpressionFactory.CreateXAggregationFromLambda(proj.Projector.Type, aggregationType, proj.Projector, proj.Source);
            }

            if (method.Arguments.Count() == 2)
            {
                var lambdaExpression = (LambdaExpression) method.Arguments[1];

                var x = Visit(method.Arguments[0]);
                ProjectionExpression proj3;
                if (x is GroupingProjection)
                {
                    proj3 = ((GroupingProjection) x).SubQueryProjectionExpression;
                }
                else
                {
                    proj3 = x as ProjectionExpression;
                }

                if (proj3 != null)
                {
                    var calculateTable = proj3.Source;
                    if (!_parameterMap.ContainsKey(lambdaExpression.Parameters[0]))
                    {
                        _parameterMap.Add(lambdaExpression.Parameters[0], proj3.Projector);
                    }

                    var body = Visit(lambdaExpression.Body);
                    ProjectedColumns e = new ColumnProjector(TabularExpressionHelper.CanBeColumn).ProjectColumns(body);
                    var grp = new ProjectionExpression(new ProjectionExpression(calculateTable, proj3.Projector), e.Projector);
                    return _columnExpressionFactory.CreateXAggregationFromLambda(method.Method.ReturnType, aggregationType, grp.Projector, grp.Source);
                }   
            }
            
            throw new NotImplementedException(method.Method.Name + method.Arguments.Count());
        }

        /// <summary>
        /// Binds take to TOPN expression with implicit ordering
        /// </summary>
        /// <param name="type">expressions runtime type</param>
        /// <param name="expression">source expression</param>
        /// <param name="n">integer constant expression for number of rows</param>
        /// <returns>bound top expression</returns>
        private Expression BindTake(Type type, Expression expression, ConstantExpression n)
        {
            var table = (expression is ProjectionExpression) ? (ProjectionExpression)expression : (ProjectionExpression)Visit(expression);
            var hasMeasure = new Finder<MeasureExpression>(); // MeasureFinder();
            ProjectedColumns pc = new ColumnProjector(TabularExpressionHelper.CanBeColumn).ProjectColumns(table.Projector);

            Expression orderby = null;

            if (_orderBys.Any())
            {
                
                var o = _orderBys.First();
                if (o.Item1 is MeasureExpression)
                {
                    orderby = new OrderByExpression(type,(MeasureExpression)o.Item1, o.Item2);
                }
                if (o.Item1 is ColumnExpression)
                {
                    orderby = new OrderByExpression(type, (ColumnExpression)o.Item1, o.Item2);
                }

            }

            if(orderby == null)
            {
                if (hasMeasure.Has(table.Source))
                {
                    orderby = new OrderByExpression(type, hasMeasure.First, OrderType.Desc);
                }
                else
                {
                    orderby = new OrderByExpression(type, (ColumnExpression) pc.Columns.First().Expression,
                        OrderType.Desc);
                }
            }
            return new ProjectionExpression(
                new TopnExpression(type, table.Source, orderby, (int)n.Value),
                table.Projector);
        }

        /// <summary>
        /// binds a GroupBy call to a summarize 
        /// </summary>
        /// <param name="type">
        /// runtime type of expression 
        /// </param>
        /// <param name="sourceExpression">
        /// base expression
        /// </param>
        /// <param name="keyLambdaExpression">
        /// The lambda Expression.
        /// </param>
        /// <param name="lambdaExpression2">
        /// The lambda Expression 2.
        /// </param>
        /// <param name="resultlambda">
        /// The lambda Expression 3.
        /// </param>
        /// <returns>
        /// Transformed group by expression
        /// </returns>
        private Expression BindGroupBy(Type type, Expression sourceExpression, LambdaExpression keyLambdaExpression, LambdaExpression elementlambda, LambdaExpression resultlambda)
        {
            ProjectionExpression projection;
            if (sourceExpression is ProjectionExpression)
            {
                projection = sourceExpression as ProjectionExpression;
            }
            else
            {
                projection = (ProjectionExpression)Visit(sourceExpression);
            }

            _parameterMap[keyLambdaExpression.Parameters[0]] = projection.Projector;
         
            Expression keybody = Visit(keyLambdaExpression.Body);
            var keyColumns = ProjectColumns(keybody);

            if ((DaxExpressionType)keybody.NodeType == DaxExpressionType.Projection)
            {
                var p = (ProjectionExpression)keybody;
                ProjectedColumns pc1 = new ColumnProjector(TabularExpressionHelper.CanBeColumn).ProjectColumns(p.Projector);
                return new ProjectionExpression(
                    new SummarizeExpression(p.Type, pc1.Columns, projection.Source),
                    p.Projector);
            }

            ProjectedColumns keyProjectedColumns = new ColumnProjector(TabularExpressionHelper.CanBeColumn).ProjectColumns(keybody);
            
            var keyProjection = new ProjectionExpression(
               DaxExpressionFactory.Create(type, keyProjectedColumns.Columns.Distinct(), projection.Source), keyProjectedColumns.Projector);

            ProjectionExpression subQueryProjection;

            if (elementlambda != null)
            {
                _parameterMap[elementlambda.Parameters[0]] = projection.Projector;
                var elements = Visit(elementlambda.Body);
                ProjectedColumns elementColumns = ProjectColumns(elements);

                subQueryProjection = new ProjectionExpression(elementlambda.Body.Type, projection,
                    elementColumns.Projector);
                


                if (resultlambda != null)
                {
                    _parameterMap[resultlambda.Parameters[0]] = keyColumns.Projector;
                    _parameterMap[resultlambda.Parameters[1]] = elementColumns.Projector;
                    Expression body3 = Visit(resultlambda.Body);
                    var resultColumns = ProjectColumns(body3);
                    subQueryProjection = new ProjectionExpression(resultlambda.Body.Type, (DaxExpression) elementColumns.Projector, resultColumns.Projector);
                }

            }
            else
            {
                if (resultlambda != null)
                {
                    _parameterMap[resultlambda.Parameters[0]] = keyProjection.Projector;
                    _parameterMap[resultlambda.Parameters[1]] = projection;
                    var results = Visit(resultlambda.Body);
                    var resultColumns = ProjectColumns(results);

                    return new ProjectionExpression(
                        DaxExpressionFactory.Create(
                            type, 
                            keyColumns.Columns.Union(resultColumns.Columns).Distinct(),
                            projection.Source)
                       , resultColumns.Projector);
                }

                subQueryProjection = projection;   
            }
            
            return new GroupingProjection(type, keyProjection, subQueryProjection);
        }

        /// <summary>
        /// Binds Select method call to summarize or some optimized form
        /// </summary>
        /// <param name="type">runtime type of the expression</param>
        /// <param name="expression">source expression</param>
        /// <param name="lambdaExpression">projection function</param>
        /// <returns>DAX expression representing the select equivalent</returns>
        private Expression BindSelect(Type type, Expression expression, LambdaExpression lambdaExpression)
        {
            var bound = Visit(expression);
            ProjectionExpression projection = null;

            //Include order by columns
            var columns = _orderBys.Select(c =>
            {
                var m = c.Item1 as MeasureExpression;
                if (m != null)
                {
                    return new MeasureDeclaration(m.Name, m.Alias, m);
                }

                var col = c.Item1 as ColumnExpression;
                if (col != null)
                {
                    return new ColumnDeclaration(col.Name, col.DbName, c.Item1);
                }
                return null;
            }).Where(x => x != null).ToList();

            switch ((DaxExpressionType) bound.NodeType)
            {
                case DaxExpressionType.Projection:
                    projection = (ProjectionExpression) bound;
                    _parameterMap[lambdaExpression.Parameters[0]] = projection.Projector;
                    break;
                case DaxExpressionType.Grouping:
                    var grp = ((GroupingProjection) bound);
                    projection = grp.SubQueryProjectionExpression;
                    _parameterMap[lambdaExpression.Parameters[0]] = bound;
                    var cs = ProjectColumns(grp.KeyProjection.Projector).Columns;
                    columns = columns.Union(cs).ToList();
                    break;
            }

            if (projection != null)
            {
                Expression body = Visit(lambdaExpression.Body);
                ProjectedColumns pc = new ColumnProjector(TabularExpressionHelper.CanBeColumn).ProjectColumns(body);

                if ((DaxExpressionType) body.NodeType == DaxExpressionType.Projection)
                {
                    var p = (ProjectionExpression) body;
                    ProjectedColumns pc1 =
                        new ColumnProjector(TabularExpressionHelper.CanBeColumn).ProjectColumns(p.Projector);
                    var summarize = new ProjectionExpression(
                        DaxExpressionFactory.Create(p.Type,pc1.Columns,projection.Source),
                        p.Projector);

                    if (lambdaExpression.ReturnType.Name.Contains("IQueryable"))
                    {
                        var elementType = TypeSystem.GetElementType(lambdaExpression.ReturnType);
                        var t = typeof(IQueryable<>).MakeGenericType(lambdaExpression.ReturnType);
                        var subquery = new SubQueryProjection(lambdaExpression.ReturnType, p);
                        return new ProjectionExpression(
                            DaxExpressionFactory.Create(t,new List<ColumnDeclaration>().AsReadOnly(), projection.Source),subquery);
                    }
                    
                    return summarize;
                }

                

                return new ProjectionExpression(
                    DaxExpressionFactory.Create(body.Type, pc.Columns.Union(columns).Distinct(), projection.Source), pc.Projector);
            }

            throw new TabularException("Unprojectible");
        }

        //internal Expression GroupingProjectionExpression(Expression expression, LambdaExpression lambdaExpression)
        //{
        //    ProjectionExpression projection;
        //    Type collectionType = null;
        //    if (expression.Type.GetGenericArguments().Count() == 2)
        //    {
        //        //IGrouping
        //        collectionType = expression.Type.GetGenericArguments()[1];
        //    }

        //    //if (expression.Type.GetGenericArguments().Count() == 1)
        //    //{
        //    //    //IEnumerable
        //    //    collectionType = expression.Type.GetGenericArguments()[0];
        //    //}

        //    if (collectionType == null)
        //    {
        //        throw new ArgumentException("Invalid expression for grouping", "expression");
        //    }

        //    var table = _parameterMap.FirstOrDefault(x => x.Value.Type == collectionType);

        //    var exp = table.Value;
        //    var daxexpression = TableFactory.GetTableExpression(exp) as TableExpression;
        //    ProjectedColumns e =
        //        new ColumnProjector(TabularExpressionHelper.CanBeColumn).ProjectColumns(Visit(lambdaExpression.Body));
        //    projection = new ProjectionExpression(expression.Type, new ProjectionExpression(daxexpression, exp), e.Projector);
        //    return projection;
        //}


        /// <summary>
        /// Binds select many as a fake join
        /// </summary>
        /// <param name="type">runtime type</param>
        /// <param name="expression">source expression</param>
        /// <param name="lambda">first projection reference</param>
        /// <param name="third">second projection reference continuation with transparent identifiers</param>
        /// <returns>summarize expression</returns>
        private Expression BindSelectMany(Type type, Expression expression, LambdaExpression lambda, LambdaExpression third)
        {
            
            var projection1 = Visit(expression);
            //make this dependent on the argument count
            
            var projection2 = Visit(lambda.Body);

            if (projection1 is GroupingProjection)
            {
                projection1 = ((GroupingProjection) projection1).SubQueryProjectionExpression;
            }

            if (projection2 is GroupingProjection)
            {
                projection2 = ((GroupingProjection)projection2).SubQueryProjectionExpression;
            }

            _parameterMap[lambda.Parameters[0]] = projection1;

            Expression firstProjection, secondProjection;
            if (third != null)
            {
                bool firstFound = _parameterMap.TryGetValue(third.Parameters[0], out firstProjection);
                bool secondFound = _parameterMap.TryGetValue(third.Parameters[1], out secondProjection);
                if (!firstFound)
                {
                    _parameterMap[third.Parameters[0]] = ((ProjectionExpression) projection1).Projector;
                }

                if (!secondFound)
                {
                    _parameterMap[third.Parameters[1]] = ((ProjectionExpression) projection2).Projector;
                }

                Expression body = Visit(third.Body);

                ProjectedColumns pc = ProjectColumns(body);

                bool isOnlyMediating = body.Type.GenericTypeArguments.Length > 0 &&
                                       body.Type.GenericTypeArguments.All(TypeSystem.AreOnly<ITabularData>);

                if (isOnlyMediating)
                {
                    return new ProjectionExpression(
                        DaxExpressionFactory.Create(
                            type,
                            new ReadOnlyCollection<ColumnDeclaration>(new ColumnDeclaration[] {}),
                            ((ProjectionExpression) projection2).Source),
                        pc.Projector);
                }

                return new ProjectionExpression(
                    DaxExpressionFactory.Create(
                        type,
                        new ReadOnlyCollection<ColumnDeclaration>(pc.Columns),
                        ((ProjectionExpression) projection2).Source),
                    pc.Projector);
            }

            return Visit(projection2);
        }

        /// <summary>
        /// Binds Where call in the LINQ expression tree
        /// </summary>
        /// <param name="resultType">runtime type</param>
        /// <param name="source">source expression</param>
        /// <param name="predicate">filtering expression</param>
        /// <returns>a DAX projection expression, CALCULATETABLE or FILTER expressions depending on the predicate</returns>
        private Expression BindWhere(Type resultType, Expression source, LambdaExpression predicate)
        {
            ProjectionExpression projection = null;
            if (source.Type.Name.StartsWith("IGrouping"))
            {
                var groupProjection = (GroupingProjection) Visit(source);
                projection = groupProjection.SubQueryProjectionExpression;
                //return new ProjectionExpression(
                //    new CalculateTableExpression(groupProjection.Type, groupProjection.SubQueryProjectionExpression.Source,groupProjection.SubQueryProjectionExpression.Projector), groupProjection.SubQueryProjectionExpression.Projector);
            }
            else
            {
                projection = (ProjectionExpression) Visit(source);
            }
            _parameterMap[predicate.Parameters[0]] = projection.Projector;
            var mainTable = CreateCalculateTable(resultType, predicate, projection);

            return new ProjectionExpression(mainTable, projection.Projector);
        }

        internal DaxExpression CreateCalculateTable(Type resultType, LambdaExpression predicate, ProjectionExpression projection)
        {
            Expression where = Visit(predicate.Body);
            FilterConditionVisitor filterConditionVisitor = FilterConditionVisitor.CreateDefault();
            filterConditionVisitor.Visit(@where);
            var fcondition = filterConditionVisitor.FilterCondition;
            var ctcondition = filterConditionVisitor.CalculateTableCondition;

            Func<DaxExpression, DaxExpression> filterFunc = s =>
            {
                if (fcondition != null)
                {
                    var projectedColumns = _columnProjector.ProjectColumns(fcondition);
                    return new FilterExpression(resultType, s, fcondition, projectedColumns.Columns);
                }

                return s;
            };

            Func<DaxExpression, DaxExpression> calculateTableFunc = s =>
            {
                if (ctcondition != null)
                {
                    return new CalculateTableExpression(resultType, s, ctcondition);
                }

                return s;
            };

            var mainTable = filterFunc(calculateTableFunc(projection.Source));
            return mainTable;
        }

        // TODO: Create Member finder visitor

        /// <summary>
        /// Method to find specific members in an expression
        /// </summary>
        /// <param name="member">member expression</param>
        /// <param name="propertyInfo">property information on the requested member</param>
        /// <returns>member expression for the corresponding property</returns>
        private Expression FindMember(Expression member, PropertyInfo propertyInfo)
        {
            if (member.Type == propertyInfo.PropertyType)
            {
                return member;
            }

            if ((DaxExpressionType)member.NodeType == DaxExpressionType.Projection)
            {
                return FindMember(((ProjectionExpression)member).Projector, propertyInfo);
            }

            if (member.NodeType == ExpressionType.MemberInit)
            {
                    var init = member as MemberInitExpression;
                    if (init == null)
                    {
                        return null;
                    }

                    foreach (MemberBinding binding in init.Bindings)
                    {
                        switch (binding.BindingType)
                        {
                            case MemberBindingType.Assignment:
                                if (((MemberAssignment)binding).Expression.Type == propertyInfo.PropertyType)
                                {
                                    if (binding.Member.Name == propertyInfo.Name)
                                    {
                                        return ((MemberAssignment) binding).Expression;
                                    }
                                }
                                
                                if (((MemberAssignment)binding).Expression.Type == propertyInfo.DeclaringType)
                                {
                                    return FindMember(((MemberAssignment)binding).Expression, propertyInfo);
                                }

                                break;
                        }
                    }
            }

            if (member.NodeType == ExpressionType.New)
            {
                var n = member as NewExpression;
                if (n != null)
                {
                    var i = n.Members.IndexOf(propertyInfo);
                    if (i >= 0)
                    {
                        var m = n.Arguments[i];
                        return m;
                    }

                    ///Check property in the nested objects
                    var bindings = 
                        n.Arguments
                        .Select(a => (MemberInitExpression) a)
                        .SelectMany(b => b != null ? b.Bindings : new ReadOnlyCollection<MemberBinding>(new MemberBinding[] { }));
                    foreach (var memberBinding in bindings)
                    {
                        if (memberBinding.Member == propertyInfo)
                        {
                            switch (memberBinding.BindingType)
                            {
                                case MemberBindingType.MemberBinding:
                                    break;
                                case MemberBindingType.ListBinding:
                                    break;
                                case MemberBindingType.Assignment:
                                    return ((MemberAssignment) memberBinding).Expression;
                            }
                        }
                    }
                    
                }
            }

            if (member.NodeType == ExpressionType.Constant)
            {
                
                var type = typeof(TabularTable<>).MakeGenericType(member.Type);
                var instance = Activator.CreateInstance(type, this.Provider);
                var proj =  _tableFactory.GetTableProjection(instance as IQueryable);
                return FindMember(proj.Projector, propertyInfo);
            }

            if ((DaxExpressionType) member.NodeType == DaxExpressionType.Grouping)
            {
                var key = ((GroupingProjection) member).KeyProjection;
                var subQuery = ((GroupingProjection)member).SubQueryProjectionExpression;
                if (propertyInfo.Name == "Key")
                {
                    return key;
                }
                
                else return FindMember(subQuery, propertyInfo);

            }

            throw new TabularException(string.Format("Could not identify member {0} in {1} expression: {2}", propertyInfo.Name, member.NodeType, member.ToString()));
        }

        /// <summary>
        /// Find a parameter in the expression
        /// </summary>
        /// <param name="t">runtime type of the parameter</param>
        /// <returns>the first expression in the parameter map that is of the requested type</returns>
        private Expression FindParameter(Type t)
        {
            return _parameterMap.Select(x =>
            {
                if (x.Value.Type == t)
                {
                    return x.Value;
                }

                if (x.Value is MemberInitExpression)
                {
                    if (((MemberInitExpression)x.Value).Bindings.Select(m => m.Member).Any(m => ((PropertyInfo)m).PropertyType == t))
                    {
                        return x.Value;
                    }
                }

                if (x.Value is NewExpression)
                {
                    if (((NewExpression)x.Value).Members.Any(m => ((PropertyInfo)m).PropertyType == t))
                    {
                        return x.Value;
                    }
                }

                return null;
            }).FirstOrDefault(x => x != null);
        }
    }
}