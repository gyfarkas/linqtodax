// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TabularEvaluator.cs" company="Dealogic">
//   See LICENCE.md
// </copyright>
// <summary>
//   evaluator class to partially evaluate an expression tree.
//   This step is necessary because it translates all local variable references in the LINQ query into values
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LinqToDAX.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using LinqToDAX.Query.DAXExpression;

    /// <summary>
    /// evaluator class to partially evaluate an expression tree.
    /// This step is necessary because it translates all local variable references in the LINQ query into values
    /// </summary>
    public static class TabularEvaluator
    {
        /// <summary>
        /// Performs evaluation and replacement of independent sub-trees
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <param name="canBeEvaluated">
        /// A function that decides whether a given expression node can be part of the local
        /// function.
        /// </param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        public static Expression PartialEval(Expression expression, Func<Expression, bool> canBeEvaluated)
        {
            return new SubtreeEvaluator(new Nominator(canBeEvaluated).Nominate(expression)).Eval(expression);
        }

        /// <summary>
        ///     Performs evaluation and replacement of independent sub-trees
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        public static Expression PartialEval(Expression expression)
        {
            var daxExpression = expression as DaxExpression;
            if (daxExpression != null)
            {
                return expression;
            }

            return PartialEval(expression, CanBeEvaluatedLocally);
        }

        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            bool notParam = expression.NodeType != ExpressionType.Parameter;
            Func<Expression, bool> notMeasure = e =>
            {
                var me = e as MethodCallExpression;
                if (me != null)
                {
                    var attribute =
                        me.Method.GetCustomAttribute(typeof(TabularMeasureMappingAttribute)) as
                            TabularMeasureMappingAttribute;
                    return attribute == null;
                }

                return true;
            };
            Func<Expression, bool> notExtension =
                e =>
                {
                    if (e.NodeType == ExpressionType.Call)
                    {
                        var me = e as MethodCallExpression;
                        if (me != null)
                        {
                            return !(me.Method.DeclaringType == typeof(IQueryable)) &&
                                   !(me.Method.DeclaringType == typeof(Queryable)) &&
                                   !(me.Method.DeclaringType == typeof(TabularQueryExtensions));
                        }
                    }

                    return true;
                };
            return notParam && notMeasure(expression) && notExtension(expression);
        }
    }

    /// <summary>
    /// Evaluates & replaces sub-trees when first candidate is reached (top-down)
    /// </summary>
    internal class SubtreeEvaluator : DaxExpressionVisitor
    {
        private readonly HashSet<Expression> _candidates;

        internal SubtreeEvaluator(HashSet<Expression> candidates)
        {
            _candidates = candidates;
        }

        internal Expression Eval(Expression exp)
        {
            return Visit(exp);
        }

        public override Expression Visit(Expression exp)
        {
            if (exp == null)
            {
                return null;
            }

            if (_candidates.Contains(exp))
            {
                return Evaluate(exp);
            }

            return base.Visit(exp);
        }


        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            var bindings = new List<MemberBinding>();
            foreach (MemberBinding binding in node.Bindings)
            {
                if (binding.BindingType == MemberBindingType.Assignment)
                {
                    var assignment = binding as MemberAssignment;
                    if (assignment == null)
                    {
                        return node;
                    } //this is stupid however;
                
                    Expression e = Visit(assignment.Expression);

                    MemberAssignment b = Expression.Bind(assignment.Member, e);
                    bindings.Add(b);
                }
                else bindings.Add(binding);
            }
            return Expression.MemberInit(node.NewExpression, bindings);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(TabularQueryExtensions))
            {
                foreach (Expression arg in node.Arguments)
                {
                    Visit(arg);
                }
            }

            return base.VisitMethodCall(node);
        }

        private static Expression Evaluate(Expression e)
        {
            if (e.NodeType == ExpressionType.Constant || (DaxExpressionType)e.NodeType == DaxExpressionType.Projection)
            {
                return e;
            }

            if (e.NodeType == ExpressionType.ListInit || e.NodeType == ExpressionType.NewArrayInit || (e.NodeType == ExpressionType.New && e.Type.IsGenericType))
            {
                return e;
            }

            LambdaExpression lambda = Expression.Lambda(e);
            Delegate fn = lambda.Compile();
            return Expression.Constant(fn.DynamicInvoke(null), e.Type);
        }
    }
}