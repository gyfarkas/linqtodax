// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenerateVisitor.cs" company="">
//   
// </copyright>
// <summary>
//   Class to format a generate expression
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using LinqToDAX.Query.DAXExpression;

namespace LinqToDAX.QueryFormatter
{
    using System.Linq.Expressions;
    using LinqToDAX.Query;

    /// <summary>
    /// Class to format a generate expression
    /// </summary>
    internal class GenerateVisitor : TabularQueryFormatter
    {
        /// <summary>
        /// Generates query string part corresponding to a generate expression
        /// </summary>
        /// <param name="generateExpression">the input generate expression</param>
        /// <returns>query string part</returns>
        public string GetGenerateString(GenerateExpression generateExpression)
        {
            Visit(generateExpression);
            return Builder.ToString();
        }

        // protected override Expression VisitTable(TableExpression tableExpression)
        // {
        //    Builder.Append("Relatedtable(");
        //    Builder.Append(tableExpression.Name);
        //    Builder.Append(")");
        //    return tableExpression;
        // }

        /// <summary>
        /// Generate expression formatting method
        /// </summary>
        /// <param name="generateExpression">generate expression to be formatted</param>
        /// <returns>the original expression</returns>
        protected override Expression VisitGenerate(GenerateExpression generateExpression)
        {
            Builder.Append("GENERATE(\n");
            Visit(generateExpression.Source);
            Builder.Append(",\n");
            Visit(generateExpression.Generator);
            Builder.Append(")\n");
            return generateExpression;
        }
    }
}
