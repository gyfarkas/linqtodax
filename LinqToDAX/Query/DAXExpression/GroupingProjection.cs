using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LinqToDAX.Query.DAXExpression
{
    /// <summary>
    /// Represents a grouping clause, 
    /// if it is followed by select than it is normalized away, 
    /// otherwise it induces the execution of several subqueries for each value of the key.
    /// </summary>
    internal class GroupingProjection : DaxExpression
    {
        internal ProjectionExpression KeyProjection { get; set; }
        internal ProjectionExpression SubQueryProjectionExpression { get; set; }
        
        internal GroupingProjection(Type t, ProjectionExpression key, ProjectionExpression subQuery) :
            base((ExpressionType) DaxExpressionType.Grouping, t)
        {
            this.KeyProjection = key;
            this.SubQueryProjectionExpression = subQuery;
        }


    }
}
