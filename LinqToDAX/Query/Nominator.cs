namespace LinqToDAX.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using LinqToDAX.Query.DAXExpression;

    /// <summary>
    ///  Performs bottom-up analysis to determine which nodes can possibly
    ///  be part of an evaluated sub-tree.
    /// </summary>
    internal class Nominator : DaxExpressionVisitor
    {
        /// <summary>
        /// The function delegate to decide whether an expression can be evaluated.
        /// </summary>
        protected readonly Func<Expression, bool> CanBeEvaluated;
       
        /// <summary>
        /// stores found candidates
        /// </summary>
        private HashSet<Expression> _candidates;

        /// <summary>
        /// whether the expression cannot be evaluated
        /// </summary>
        private bool _cannotBeEvaluated;

        /// <summary>
        /// Initializes a new instance of the <see cref="Nominator" /> class.
        /// </summary>
        /// <param name="canBeEvaluated">callback that decides the interesting sub-expressions</param>
        internal Nominator(Func<Expression, bool> canBeEvaluated)
        {
            CanBeEvaluated = canBeEvaluated;
        }

        /// <summary>
        /// Visit the expression and find sub-expressions that can be evaluated according to the passed callback
        /// </summary>
        /// <param name="expression">Expression to be inspected</param>
        /// <returns>the expression unchanged</returns>
        public override Expression Visit(Expression expression)
        {
            if (expression != null)
            {
                bool saveCannotBeEvaluated = _cannotBeEvaluated;
                _cannotBeEvaluated = false;
                base.Visit(expression);
                if (!_cannotBeEvaluated)
                {
                    if (CanBeEvaluated(expression))
                    {
                        _candidates.Add(expression);
                    }
                    else
                    {
                        _cannotBeEvaluated = true;
                    }
                }

                _cannotBeEvaluated |= saveCannotBeEvaluated;
            }

            return expression;
        }

        /// <summary>
        /// Calls the visit method for the expression 
        /// and returns the candidate sub-expressions
        /// </summary>
        /// <param name="expression">Expression to be inspected</param>
        /// <returns>The candidate sub-expressions</returns>
        internal virtual HashSet<Expression> Nominate(Expression expression)
        {
            _candidates = new HashSet<Expression>();
            _cannotBeEvaluated = false;
            Visit(expression);
            return _candidates;
        }

    }
}