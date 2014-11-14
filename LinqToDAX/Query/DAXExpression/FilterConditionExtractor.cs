using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;

namespace LinqToDAX.Query.DAXExpression
{

    internal class FilterConditionExtractor :DaxExpressionVisitor
    {

        private ReadOnlyCollection<Expression> _foundArguments;

        private ReadOnlyCollection<MemberInfo> _foundMemberInfos;

        private Dictionary<MemberInfo, Tuple<Expression, Expression>> _memberMapping =new Dictionary<MemberInfo, Tuple<Expression, Expression>>(); 

        private readonly ExpressionType _nodeType;

        private Expression _firstNode;

        private Expression _secondNode;
        
        public FilterConditionExtractor(ExpressionType nodeType, Expression node1,Expression node2)
        {

            _nodeType = nodeType;
            _firstNode = node1;
            _secondNode = node2;     
        }

        public BinaryExpression Extract()
        {
            _firstNode = Visit(_firstNode);
            _secondNode = Visit(_secondNode);

            var elements = _memberMapping.Select(x => Expression.MakeBinary(_nodeType,x.Value.Item1, x.Value.Item2));
            var rootnodetype = _nodeType == ExpressionType.NotEqual ? ExpressionType.Or : ExpressionType.And;
            var exp = elements.Aggregate((x, y) => Expression.MakeBinary(rootnodetype, x, y));
            return exp;
        }

        protected override Expression VisitNew(NewExpression node)
        {

            for (var i = 0; i < node.Members.Count; i++)
            {
                if (_memberMapping.ContainsKey(node.Members[i]))
                {
                    var t1 = _memberMapping[node.Members[i]].Item1;
                    var t2 = Visit(node.Arguments[i]);
                    _memberMapping[node.Members[i]] = new Tuple<Expression, Expression>(t1, t2);
                }
                else
                {
                    _memberMapping.Add(node.Members[i],
                        new Tuple<Expression, Expression>(Visit(node.Arguments[i]), Expression.Empty()));
                }
            }
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            //return base.VisitConstant(node);
            var v = node.Value;
            var keys = _memberMapping.Keys.ToList();
            foreach (var member in keys)
            {
                var m = v.GetType().GetMember(member.Name).FirstOrDefault();
                if (m != null && m.ReflectedType == member.ReflectedType)
                {
                    var accessor = TabularEvaluator.PartialEval(Expression.MakeMemberAccess(node, m));
                    var t1 = _memberMapping[member].Item1;
                    _memberMapping[member] = new Tuple<Expression, Expression>(t1,accessor);
                } 
            }
            return node;
        }

        protected override MemberBinding VisitMemberBinding(MemberBinding node)
        {
            return base.VisitMemberBinding(node);
        }
    }
}