using System.Collections.Generic;

namespace LinqToDAX.Query.DAXExpression
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Factory that creates different ALL expressions for a query
    /// </summary>
    internal class AllExpressionFactory
    {
        /// <summary>
        /// Reference to the binder object
        /// </summary>
        private readonly TabularQueryBinder _binder;

        /// <summary>
        /// Initializes a new instance of the <see cref="AllExpressionFactory"/> class.
        /// </summary>
        /// <param name="binder">parent binder reference</param>
        public AllExpressionFactory(TabularQueryBinder binder)
        {
            _binder = binder;
        }

        /// <summary>
        /// Creates all or all selected expressions
        /// </summary>
        /// <param name="node">parent method call</param>
        /// <param name="selected">type of the all expression</param>
        /// <returns>an all expression of the appropriate type</returns>
        internal Expression CreateAllExpression(MethodCallExpression node, AllType selected)
        {
            if (node.Arguments.Count < 1)
            {
                throw new ArgumentException("Must have at least one argument");
            }

            if (node.Arguments.Count() == 1)
            {
                return GetAllExpression(node.Arguments[0], node.Method.ReturnType, selected);
            }

            Expression all = GetAllExpression(node.Arguments[0], node.Method.ReturnType, selected);
            Expression filter = _binder.Visit(node.Arguments[1]);
            return new FilterExpression(node.Method.ReturnType, all, filter, new List<ColumnDeclaration>());
        }

        /// <summary>
        /// Creates an all expression with argument 
        /// </summary>
        /// <param name="arg">argument expression for the expression created</param>
        /// <param name="returnType">runtime type</param>
        /// <param name="selected">type of the all expression to be created</param>
        /// <returns>an expression representing an ALL or an ALLSELECTED DAX function</returns>
        private Expression GetAllExpression(Expression arg, Type returnType, AllType selected)
        {
            if (arg.NodeType == ExpressionType.Parameter)
            {
                if (selected == AllType.AllSelected)
                {
                    return new AllSelectedExpression(returnType, arg);
                }
                return new AllExpression(returnType, arg);
            }

            if (arg.NodeType == ExpressionType.MemberAccess)
            {
                var m = (MemberExpression)arg;
                var te = TableFactory.GetTableExpression(m) as TableExpression;
                if (te != null && (selected == AllType.AllSelected))
                {
                    return new AllSelectedExpression(returnType, te);
                }

                if (te != null)
                {
                    return new AllExpression(returnType, te);
                }
            }

            Expression argument = _binder.Visit(arg);
            if (selected == AllType.AllSelected)
            {
                return new AllSelectedExpression(returnType, argument);
            }

            return new AllExpression(returnType, argument);
        }
    }
}