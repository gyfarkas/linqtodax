using System.Collections.Generic;
using System.Linq.Expressions;
using LinqToDAX.Query.DAXExpression;

namespace LinqToDAX.Query
{
    internal class BooleanConstantFinder : DaxExpressionVisitor
    {
        private bool _found;

        public BooleanConstantFinder()
        {
            Expressions = new List<ConstantExpression>();
        }

        public List<ConstantExpression> Expressions { get; set; }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value == null)
            {
                return node;
            }

            switch (node.Value.GetType().ToString())
            {
                case "System.Boolean":
                    _found = true;
                    Expressions.Add(node);
                    return node;
            }

            return node;
        }

        public bool FindConstant(Expression node)
        {
            Visit(node);
            return _found;
        }
    }
}