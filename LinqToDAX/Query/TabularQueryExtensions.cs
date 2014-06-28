// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TabularQueryExtensions.cs" company="Dealogic">
//   see LICENTCE.md
// </copyright>
// <summary>
//   Extension Methods for translating DAX functions in Linq queries
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LinqToDAX.Query
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using DAXExpression;
    using TabularEntities;
    
    /// <summary>
    ///     Extension Methods for translating DAX functions in Linq queries
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
    public static class TabularQueryExtensions
    {
        /// <summary>
        /// DAX CALCULATETABLE function with table filter
        /// The CALCULATETABLE function changes the context in which the data is filtered,
        /// and evaluates the expression in the new context that you specify. 
        /// For each column used in a filter argument, 
        /// any existing filters on that column are removed, and the filter used in the filter argument is applied instead.
        /// http://technet.microsoft.com/en-us/library/ee634760.aspx
        /// </summary>
        /// <param name="source">a table expression that filters the source table</param>
        /// <param name="filterTable">the table valued expression to be filtered</param>
        /// <returns>a tabular table with the data from source filtered by the filter table</returns>
        public static TabularTable<TData> CalculateTable<TData, TFilter>(this IQueryable<TData> source,
            IQueryable<TFilter> filterTable)
        {
            Expression sourceExpression = TabularEvaluator.PartialEval(source.Expression);
            Expression filterExpression = TabularEvaluator.PartialEval(filterTable.Expression);
            var projection = (ProjectionExpression) new TabularQueryBinder().Bind(sourceExpression);
            var filter = (ProjectionExpression) new TabularQueryBinder().Bind(filterExpression);
            var calculateTableExpression = new CalculateTableExpression(typeof (TData), projection.Source, filter.Source);
            Type elevatedType = typeof (TabularTable<>).MakeGenericType(projection.Projector.Type);
            var resultProjectionExpression =
                new ProjectionExpression(
                    elevatedType,
                    calculateTableExpression,
                    projection.Projector);
            return (TabularTable<TData>) source.Provider.CreateQuery<TData>(resultProjectionExpression);
        }

        /// <summary>
        /// Returns a table with the Cartesian product between each row in table1 
        /// and the table that results from evaluating table 2 in the context of the current row from table1.
        /// GENERATE(<table1>, <table2>)
        /// http://technet.microsoft.com/en-us/library/gg492196.aspx
        /// </summary>
        /// <param name="source">table 1 in the GENERATE DAX function</param>
        /// <param name="generator">table 2  in the GENERATE DAX function</param>
        /// <param name="selectorFunc">function to select the result</param>
        public static TabularTable<TData> Generate<TData, TSource, TGenerator>(
            this IQueryable<TSource> source,
            IQueryable<TGenerator> generator,
            Expression<Func<TSource, TGenerator, TData>> selectorFunc)
        {
            var binder = new TabularQueryBinder();
            Expression sourceExpression = TabularEvaluator.PartialEval(source.Expression);
            Expression generatorExpression = TabularEvaluator.PartialEval(generator.Expression);
            var resultExpression = binder.BindGenerate(sourceExpression, generatorExpression, selectorFunc);
            return (TabularTable<TData>) source.Provider.CreateQuery<TData>(resultExpression);
        }

        
        public static T Average<T>(this T column)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "column"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "filter")]
        public static T Average<T>(this T column, bool filter)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "column"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "filterExp")]
        public static T Average<T>(this T column, ITabularData filterExp)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "column")]
        public static T Sum<T>(this T column)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "column"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "filter")]
        public static T Sum<T>(this T column, bool filter)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "column"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "filterExp")]
        public static T Sum<T>(this T column, ITabularData filterExp)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "column")]
        public static T Max<T>(this T column)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "column"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "filter")]
        public static T Max<T>(this T column, bool filter)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "filterExp"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "column")]
        public static T Max<T>(this T column, ITabularData filterExp)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "column")]
        public static T Min<T>(this T column)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "column"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "filter")]
        public static T Min<T>(this T column, bool filter)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "filterExp"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "column")]
        public static T Min<T>(this T column, ITabularData filterExp)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "column")]
        public static long DistinctCount<T>(this T column)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "filter"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "column")]
        public static long? DistinctCount<T>(this T column, bool filter)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "column"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "filterExp")]
        public static long? DistinctCount<T>(this T column, ITabularData filterExp)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// Creates a LOOKUPVALUE function in the DAX expression, 
        /// LOOKUPVALUE( result_columnName, search_columnName, search_value[, search_columnName, search_value]…)
        /// Returns the value in result_columnName for the row that meets all criteria specified by search_columnName and search_value.
        /// <see cref="http://technet.microsoft.com/en-us/library/gg492170.aspx"></see>
        /// </summary>
        /// <param name="target">should be a property access referring to a column</param>
        /// <param name="lookupdict">should be an even number of column referring properties of some table</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "lookupdict")]
        public static T LookupValue<T, TSearch>(this T target, params TSearch[] lookupdict)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// Creates a UseRelationship function in the DAX expression.
        /// </summary>
        /// <typeparam name="T">type of related columns</typeparam>
        /// <param name="source">source of the relationship</param>
        /// <param name="target">target of the relationship</param>
        /// <returns>virtually a boolean to be applicable inside where</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "source"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target")]
        public static bool UseRelationship<T>(this T source, T target)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// Creates a UseRelationship function in the DAX expression by giving a column name as string.
        /// Useful to parameterize relationships in code.
        /// </summary>
        /// <typeparam name="T">type of related columns</typeparam>
        /// <param name="source">source of the relationship</param>
        /// <param name="target">target of the relationship</param>
        /// <returns>virtually a boolean to be applicable inside where</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "source"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target")]
        public static bool ApplyRelationship<T>(this T source, string target)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        ///  Returns all the rows in a table, or all the values in a column, 
        /// ignoring any filters that might have been applied. 
        /// This function is useful for clearing filters and creating calculations on all the rows in a table.
        /// http://technet.microsoft.com/en-us/library/ee634802.aspx
        /// </summary>
        /// <returns>boolean, the return value is never used, it is meant to be part of a filter expression in TabularTable Where()</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "exp")]
        public static bool ForAll<T>(this T exp)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// The ALLSELECTED function gets the context that represents all rows and columns in the query,
        ///  while keeping explicit filters and contexts other than row and column filters. 
        /// This function can be used to obtain visual totals in queries.
        ///  http://technet.microsoft.com/en-us/library/gg492186.aspx
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "exp")]
        public static bool ForAllSelected<T>(this T exp)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// 
        /// Returns all the rows in a table, or all the values in a column, 
        /// ignoring any filters that might have been applied. 
        /// This function is useful for clearing filters and creating calculations on all the rows in a table.
        /// This version generates a FILTER(ALL(exp) && cond) form in DAX, that is fairly common
        /// http://technet.microsoft.com/en-us/library/ee634802.aspx
        /// </summary>
        /// <returns>boolean, the return value is never used, it is meant to be part of a filter expression in TabularTable Where()</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "conditions"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "exp")]
        public static bool ForAll<T>(this T exp, bool conditions)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// 
        /// The ALLSELECTED function gets the context that represents all rows and columns in the query,
        ///  while keeping explicit filters and contexts other than row and column filters. 
        /// This function can be used to obtain visual totals in queries.
        /// This version generates a FILTER(ALLSELECTED(exp) && cond) form in DAX, that is fairly common
        /// http://technet.microsoft.com/en-us/library/gg492186.aspx
        /// </summary>
        /// <returns>boolean, the return value is never used, it is meant to be part of a filter expression in TabularTable Where()</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "exp"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "conditions")]
        public static bool ForAllSelected<T>(this T exp, bool conditions)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// Create SUMX(table, expression) DAX Expression 
        /// Returns the sum of an expression evaluated for each row in a table.
        /// http://technet.microsoft.com/en-us/library/ee634959.aspx
        /// </summary>
        /// <param name="table">The table containing the rows for which the expression will be evaluated.</param>
        /// <param name="projector">The expression to be evaluated for each row of the table.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "projector"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "table")]
        public static TValue? Sumx<T, TValue>(this IQueryable<T> table, Func<T, TValue> projector) where TValue : struct
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// Creates MAXX(table,expression) DAX expression
        /// Evaluates an expression for each row of a table and returns the largest numeric value.
        /// http://technet.microsoft.com/en-us/library/ee634576.aspx
        /// </summary>
        /// <param name="table">The table containing the rows for which the expression will be evaluated.</param>
        /// <param name="projector">The expression to be evaluated for each row of the table.</param>
        /// <returns>the maximum value of the result of the projection on the table</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "projector"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "table")]
        public static TValue? Maxx<T, TValue>(this IQueryable<T> table, Func<T, TValue> projector) where TValue : struct
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// Create SUMX(table, expression) DAX Expression 
        /// Returns the sum of an expression evaluated for each row in a table.
        /// http://technet.microsoft.com/en-us/library/ee634959.aspx
        /// </summary>
        /// <param name="table">The table containing the rows for which the expression will be evaluated.</param>
        /// <param name="projector">The expression to be evaluated for each row of the table.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "table"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "projector")]
        public static TValue? Sumx<T, TValue>(this IQueryable<T> table, Func<T, TValue?> projector)
            where TValue : struct
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// Creates MAXX(table,expression) DAX expression
        /// Evaluates an expression for each row of a table and returns the largest numeric value.
        /// http://technet.microsoft.com/en-us/library/ee634576.aspx
        /// </summary>
        /// <param name="table">The table containing the rows for which the expression will be evaluated.</param>
        /// <param name="projector">The expression to be evaluated for each row of the table.</param>
        /// <returns>the maximum value of the result of the projection on the table</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "table"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "projector")]
        public static TValue? Maxx<T, TValue>(this IQueryable<T> table, Func<T, TValue?> projector)
            where TValue : struct
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// Creates MINX(table,expression) DAX expression
        /// Returns the smallest numeric value that results from evaluating an expression for each row of a table.
        /// <see cref="http://technet.microsoft.com/en-us/library/ee634545.aspx"></see>
        /// </summary>
        /// <param name="table">The table containing the rows for which the expression will be evaluated.</param>
        /// <param name="projector">The expression to be evaluated for each row of the table.</param>
        /// <returns>the minimum value of the result of the projection on the table</returns>
        /// <typeparam name="T">type of the table</typeparam>
        /// <typeparam name="TValue">type of the value returned</typeparam>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "projector"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "table")]
        public static TValue? Minx<T, TValue>(this IQueryable<T> table, Func<T, TValue> projector) where TValue : struct
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// Creates MINX(table,expression) DAX expression
        /// Returns the smallest numeric value that results from evaluating an expression for each row of a table.
        /// http://technet.microsoft.com/en-us/library/ee634545.aspx
        /// </summary>
        /// <param name="table">The table containing the rows for which the expression will be evaluated.</param>
        /// <param name="projector">The expression to be evaluated for each row of the table.</param>
        /// <returns>the minimum value of the result of the projection on the table</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "projector"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "table")]
        public static TValue? Minx<T, TValue>(this IQueryable<T> table, Func<T, TValue?> projector)
            where TValue : struct
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// Creates a RANKX(table, expression) DAX expression (value, order and ties arguments are not supported yet)
        /// Returns the ranking of a number in a list of numbers for each row in the table argument.
        /// http://technet.microsoft.com/en-us/library/gg492185.aspx
        /// </summary>
        /// <param name="table">DAX expression that returns a table of data over which the expression is evaluated..</param>
        /// <param name="projector">The expression to be evaluated for each row of the table.</param>
        /// <returns>ranking of a number </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "projector"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "table")]
        public static long? Rankx<T, TValue>(this IQueryable<T> table, Func<T, TValue> projector)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// Shortcut for reverse ranking by taking -expression as ranking,
        /// (this function will be removed, when ran order will be supported in rankx)
        /// Creates a RANKX(table, -expression) DAX expression (value, order and ties arguments are not supported yet)
        /// Returns the ranking of a number in a list of numbers for each row in the table argument.
        /// http://technet.microsoft.com/en-us/library/gg492185.aspx
        /// </summary>
        /// <param name="table">DAX expression that returns a table of data over which the expression is evaluated..</param>
        /// <param name="projector">The expression to be evaluated for each row of the table.</param>
        /// <returns>ranking of a number </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "projector"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "table")]
        public static long? ReverseRankx<T, TValue>(this IQueryable<T> table, Func<T, TValue> projector)
            where TValue : struct
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// AVERAGEX function call.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="table"></param>
        /// <param name="projector"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "projector"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "table")]
        public static TValue Averagex<T, TValue>(this IQueryable<T> table, Func<T, TValue> projector)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// COUNTX function call.
        /// </summary>
        /// <typeparam name="T">type of the table</typeparam>
        /// <typeparam name="TValue">type of the value counted</typeparam>
        /// <param name="table">Tabular table or table expression</param>
        /// <param name="projector">function to select the value to count, must be column reference</param>
        /// <returns>count of values</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "projector"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "table")]
        public static long? Countx<T, TValue>(this IQueryable<T> table, Func<T, TValue> projector)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// COUNTROWS function
        /// </summary>
        /// <typeparam name="T">Type of tabular table</typeparam>
        /// <param name="table">table to be counted</param>
        /// <returns>number of rows</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "table")]
        public static long? CountRows<T>(this IQueryable<T> table)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }
    }
}