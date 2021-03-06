﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FilterConditionVisitor.cs" company="Dealogic">
//   See LICENCE.md
// </copyright>
// <summary>
//   Defines the FilterConditionVisitor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LinqToDAX.Query.DAXExpression
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// This class separates boolean expressions in a lambda expression
    /// to to parts, one that can be in CALCULATETABLE and another that should go to FILTER 
    /// </summary>
    internal class FilterConditionVisitor : DaxExpressionVisitor
    {
        /// <summary>
        /// Expressions for FILTER
        /// </summary>
        private readonly List<Expression> _filterConditions;

        /// <summary>
        /// Expressions for CALCULATETABLE
        /// </summary>
        private readonly List<Expression> _calculateTableConditions;

        private readonly Func<Expression, bool> _isFilterCondition;

        private readonly Func<Expression, Expression, bool> _hasSameColumn;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterConditionVisitor"/> class. 
        /// </summary>
        /// <param name="isFilterCondition">delegate to decide whether the expression should be in FILTER or CALCULATETABLE</param>
        /// <param name="hasSameColumn">delegate that decides if the expression refers to a single column</param>
        internal FilterConditionVisitor(Func<Expression, bool> isFilterCondition, Func<Expression, Expression, bool> hasSameColumn)
        {
            _filterConditions = new List<Expression>();
            _calculateTableConditions = new List<Expression>();
            _isFilterCondition = isFilterCondition;
            _hasSameColumn = hasSameColumn;
        }

        /// <summary>
        /// Gets the conditions that may go to the query CALCULATETABLE function
        /// </summary>
        internal Expression CalculateTableCondition
        {
            get
            {
                if (_calculateTableConditions.Any())
                {
                    return _calculateTableConditions.Distinct().Aggregate(Expression.And);
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the conditions that can only be in FILTER function
        /// </summary>
        internal Expression FilterCondition
        {
            get
            {
                if (_filterConditions.Any())
                {
                    return _filterConditions.Distinct().Aggregate(Expression.And);
                }

                return null;
            }
        }

        /// <summary>
        /// Creates a default instance with the delegates defined
        /// </summary>
        /// <returns>A new instance of the visitor class with default delegates</returns>
        internal static FilterConditionVisitor CreateDefault()
        {
            Func<Expression, bool> isFilterCondition = expression =>
            {
                var node = expression as UnaryExpression;
                if (node != null)
                {
                    expression = node.Operand;
                }

                switch ((DaxExpressionType)expression.NodeType)
                {
                    case DaxExpressionType.Column:
                    case DaxExpressionType.UseRelationship:
                    case DaxExpressionType.All:
                        return false;   
                    case DaxExpressionType.XAggregation:
                    case DaxExpressionType.Lookup:
                    case DaxExpressionType.Measure:
                        return true;
                }


                return false;
            };

            Func<Expression, Expression, bool> hasSameColumn = (left, right) =>
            {
                var leftExp = left as BinaryExpression;
                var rightExp = right as BinaryExpression;
                if (leftExp != null && rightExp != null)
                {
                    var leftColumn = leftExp.Left as ColumnExpression ?? rightExp.Right as ColumnExpression;
                    var rightColumn = rightExp.Left as ColumnExpression ?? rightExp.Right as ColumnExpression;

                    if (leftColumn != null && rightColumn != null)
                    {
                        return leftColumn.DbName == rightColumn.DbName && leftColumn.TableName == rightColumn.TableName;
                    }
                }

                return false;
            };

            return new FilterConditionVisitor(isFilterCondition, hasSameColumn);
        }

        /// <summary>
        /// Adds use relationship to calculate table conditions.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitUseRelationship(UseRelationshipExpression node)
        {
            _calculateTableConditions.Add(node);
            return node;
        }

        protected override Expression VisitAllExpression(AllExpression node)
        {
            _calculateTableConditions.Add(node);
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression constant)
        {
            if (constant.Type == typeof(bool))
            {
                _filterConditions.Add(constant);
                return constant;
            }

            return base.VisitConstant(constant);
        }

        /// <summary>
        /// Examine Binary expressions
        /// </summary>
        /// <param name="node">a binary expression</param>
        /// <returns>the expression unchanged</returns>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if ((node.Left is NewExpression || node.Left is ConstantExpression)
                        && (node.Right is NewExpression || node.Right is ConstantExpression))
            {
                var extractor = new FilterConditionExtractor(node.NodeType, node.Left, node.Right);
                var extracted = extractor.Extract();
                return Visit(extracted);
            }
            switch (node.NodeType)
            {
                case ExpressionType.AndAlso:
                    Visit(node.Left);
                    Visit(node.Right);
                    break;
                case ExpressionType.NotEqual:
                    //_filterConditions.Add(node);
                    //return node;
                case ExpressionType.Equal:
                case ExpressionType.LessThan:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThanOrEqual:
                    var  bothColumns = node.Left is ColumnExpression && node.Right is ColumnExpression;
                    
                    if (_isFilterCondition(node.Left) 
                        || _isFilterCondition(node.Right) 
                        || (bothColumns && !_hasSameColumn(node.Left, node.Right)))
                    {
                        _filterConditions.Add(node);
                        return node;
                    }
                    //if (bothColumns)
                   // {
                        _calculateTableConditions.Add(node);
                        return node;
                   // }
                   // break;

                case ExpressionType.OrElse:
                    if (_isFilterCondition(node.Left) || _isFilterCondition(node.Right))
                    {
                        _filterConditions.Add(node);
                        return node;
                    }

                    if (_hasSameColumn(node.Left, node.Right))
                    {
                        _calculateTableConditions.Add(node);
                        return node;
                    }
                    
                    _filterConditions.Add(node);
                    return node;
            }

            return base.VisitBinary(node);
        }
    }
}
