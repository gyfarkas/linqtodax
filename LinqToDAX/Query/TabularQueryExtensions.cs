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
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using DAXExpression;
    using TabularEntities;

    /// <summary>
    ///     Extension Methods for translating DAX functions in LINQ queries 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
        Justification = "Methods are translated to DAX")]
    public static class TabularQueryExtensions
    {
        /// <summary>
        /// DAX CALCULATETABLE function with table filter
        /// The CALCULATETABLE function changes the context in which the data is filtered,
        /// and evaluates the expression in the new context that you specify. 
        /// For each column used in a filter argument, 
        /// any existing filters on that column are removed, and the filter used in the filter argument is applied instead.
        /// <seealso cref="http://technet.microsoft.com/en-us/library/ee634760.aspx" />
        /// </summary>
        /// <param name="source">a table expression that filters the source table</param>
        /// <param name="filterTable">the table valued expression to be filtered</param>
        /// <typeparam name="TData">type of table elements</typeparam>
        /// <typeparam name="TFilter">Filter query type</typeparam>
        /// <returns>a tabular table with the data from source filtered by the filter table</returns>
        public static IQueryable<TData> CalculateTable<TData, TFilter>(this IQueryable<TData> source, IQueryable<TFilter> filterTable)
        {
            Expression sourceExpression = TabularEvaluator.PartialEval(source.Expression);
            Expression filterExpression = TabularEvaluator.PartialEval(filterTable.Expression);
            var projection = (ProjectionExpression)new TabularQueryBinder().Bind(sourceExpression);
            var filter = (ProjectionExpression)new TabularQueryBinder().Bind(filterExpression);
            var calculateTableExpression = new CalculateTableExpression(typeof(TData), projection.Source, filter.Source);
            Type elevatedType = typeof(TabularTable<>).MakeGenericType(projection.Projector.Type);
            var resultProjectionExpression =
                new ProjectionExpression(
                    elevatedType,
                    calculateTableExpression,
                    projection.Projector);
            return source.Provider.CreateQuery<TData>(resultProjectionExpression);
        }

        /// <summary>
        /// Returns a table with the Cartesian product between each row in table1 
        /// and the table that results from evaluating table 2 in the context of the current row from table1.
        /// GENERATE(table1, table2)
        /// <seealso cref="http://technet.microsoft.com/en-us/library/gg492196.aspx" />
        /// </summary>
        /// <param name="source">table 1 in the GENERATE DAX function</param>
        /// <param name="generator">table 2  in the GENERATE DAX function</param>
        /// <param name="selectorFunc">function to select the result</param>
        /// <typeparam name="TData">result table type</typeparam>
        /// <typeparam name="TSource">first table type</typeparam>
        /// <typeparam name="TGenerator">Generator table type</typeparam>
        /// <returns>a new tabular table</returns>
        public static IQueryable<TData> Generate<TData, TSource, TGenerator>(
            this IQueryable<TSource> source,
            IQueryable<TGenerator> generator,
            Expression<Func<TSource, TGenerator, TData>> selectorFunc)
        {
            var binder = new TabularQueryBinder();
            Expression sourceExpression = TabularEvaluator.PartialEval(source.Expression);
            Expression generatorExpression = TabularEvaluator.PartialEval(generator.Expression);
            var resultExpression = binder.BindGenerate(sourceExpression, generatorExpression, selectorFunc);
            return source.Provider.CreateQuery<TData>(resultExpression);
        }

        /// <summary>
        /// AVERAGE function
        /// </summary>
        /// <typeparam name="T">type of the column and the result</typeparam>
        /// <param name="column">column reference in LINQ expression</param>
        /// <returns>average of column values</returns>
        public static T Average<T>(this T column)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// Filtered AVERAGE function
        /// </summary>
        /// <typeparam name="T">column and result type</typeparam>
        /// <param name="column">LINQ column reference</param>
        /// <param name="filter">filter expression</param>
        /// <returns>average of the filtered values</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "column", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "filter", Justification = "Method is translated")]
        public static T Average<T>(this T column, bool filter)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// The average.
        /// </summary>
        /// <param name="column">
        /// The column.
        /// </param>
        /// <param name="filterExp">
        /// The filter expression
        /// </param>
        /// <typeparam name="T">
        /// column type 
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "column", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "filterExp", Justification = "Method is translated")]
        public static T Average<T>(this T column, ITabularData filterExp)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// The sum.
        /// </summary>
        /// <param name="column">
        /// The column.
        /// </param>
        /// <typeparam name="T">
        /// column type
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "column", Justification = "Method is translated")]
        public static T Sum<T>(this T column)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// The sum.
        /// </summary>
        /// <param name="column">
        /// The column.
        /// </param>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <typeparam name="T">
        /// column type
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "column", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "filter", Justification = "Method is translated")]
        public static T Sum<T>(this T column, bool filter)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// The sum.
        /// </summary>
        /// <param name="column">
        /// The column.
        /// </param>
        /// <param name="filterExp">
        /// The filter expression.
        /// </param>
        /// <typeparam name="T">
        /// column type
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "column", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "filterExp", Justification = "Method is translated")]
        public static T Sum<T>(this T column, ITabularData filterExp)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// The max.
        /// </summary>
        /// <param name="column">
        /// The column.
        /// </param>
        /// <typeparam name="T">
        /// column type
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "column", Justification = "Method is translated")]
        public static T Max<T>(this T column)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// The max.
        /// </summary>
        /// <param name="column">
        /// The column.
        /// </param>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <typeparam name="T">
        /// column type
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "column", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "filter", Justification = "Method is translated")]
        public static T Max<T>(this T column, bool filter)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// The max.
        /// </summary>
        /// <param name="column">
        /// The column.
        /// </param>
        /// <param name="filterExp">
        /// The filter expression.
        /// </param>
        /// <typeparam name="T">
        /// column type
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "filterExp", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "column", Justification = "Method is translated")]
        public static T Max<T>(this T column, ITabularData filterExp)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// The min.
        /// </summary>
        /// <param name="column">
        /// The column.
        /// </param>
        /// <typeparam name="T">
        /// column type
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "column", Justification = "Method is translated")]
        public static T Min<T>(this T column)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// The min.
        /// </summary>
        /// <param name="column">
        /// The column.
        /// </param>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <typeparam name="T">
        /// column type
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "column", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "filter", Justification = "Method is translated")]
        public static T Min<T>(this T column, bool filter)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// The min.
        /// </summary>
        /// <param name="column">
        /// The column.
        /// </param>
        /// <param name="filterExp">
        /// The filter expression.
        /// </param>
        /// <typeparam name="T">
        /// column type
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "filterExp", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "column", Justification = "Method is translated")]
        public static T Min<T>(this T column, ITabularData filterExp)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// The distinct count.
        /// </summary>
        /// <param name="column">
        /// The column.
        /// </param>
        /// <typeparam name="T">
        /// column type 
        /// </typeparam>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "column", Justification = "Method is translated")]
        public static long DistinctCount<T>(this T column)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// The distinct count.
        /// </summary>
        /// <param name="column">
        /// The column.
        /// </param>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <typeparam name="T">
        /// column type 
        /// </typeparam>
        /// <returns>
        /// long result or null
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "filter", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "column", Justification = "Method is translated")]
        public static long? DistinctCount<T>(this T column, bool filter)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// The distinct count.
        /// </summary>
        /// <param name="column">
        /// The column.
        /// </param>
        /// <param name="filterExp">
        /// The filter expression. 
        /// </param>
        /// <typeparam name="T">
        /// column type 
        /// </typeparam>
        /// <returns>
        /// The <see cref="long"/> result or null.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "column", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "filterExp", Justification = "Method is translated")]
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
        /// <typeparam name="T">
        /// target type
        /// </typeparam>
        /// <typeparam name="TSearch">
        /// lookup type
        /// </typeparam>
        /// <param name="target">
        /// should be a property access referring to a column
        /// </param>
        /// <param name="lookupdict">
        /// should be an even number of column referring properties of some table
        /// </param>
        /// <returns>
        /// target value 
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "target", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "lookupdict", Justification = "Method is translated")]
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "source", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "target", Justification = "Method is translated")]
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "source", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "target", Justification = "Method is translated")]
        public static bool ApplyRelationship<T>(this T source, string target)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        ///  Returns all the rows in a table, or all the values in a column, 
        /// ignoring any filters that might have been applied. 
        /// This function is useful for clearing filters and creating calculations on all the rows in a table.
        /// <seealso cref="http://technet.microsoft.com/en-us/library/ee634802.aspx" /> 
        /// </summary>
        /// <typeparam name="T">column type</typeparam>
        /// <param name="exp">column or table reference</param>
        /// <returns>boolean, the return value is never used, it is meant to be part of a filter expression in TabularTable Where()</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "exp", Justification = "Method is translated")]
        public static bool ForAll<T>(this T exp)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// The ALLSELECTED function gets the context that represents all rows and columns in the query,
        ///  while keeping explicit filters and contexts other than row and column filters. 
        /// This function can be used to obtain visual totals in queries.
        ///  <seealso cref="http://technet.microsoft.com/en-us/library/gg492186.aspx"/>
        /// </summary>
        /// <typeparam name="T">column type</typeparam>
        /// <param name="exp">column or table reference</param>
        /// <returns>boolean, the return value is never used, it is meant to be part of a filter expression in a TabularTable query Where()</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "exp", Justification = "Method is translated")]
        public static bool ForAllSelected<T>(this T exp)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// Returns all the rows in a table, or all the values in a column, 
        /// ignoring any filters that might have been applied. 
        /// This function is useful for clearing filters and creating calculations on all the rows in a table.
        /// This version generates a FILTER(ALL(EXP) &amp;&amp; COND) form in DAX, that is fairly common  
        /// </summary>
        /// <seealso cref="http://technet.microsoft.com/en-us/library/ee634802.aspx" />
        /// <param name="exp">expression table or column reference</param>
        /// <param name="conditions">filters the table by these, conditions are translated</param>
        /// <typeparam name="T">expression type</typeparam>
        /// <returns>boolean, the return value is never used, it is meant to be part of a filter expression in TabularTable Where()</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "conditions", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "exp", Justification = "Method is translated")]
        public static bool ForAll<T>(this T exp, bool conditions)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// The ALLSELECTED function gets the context that represents all rows and columns in the query,
        ///  while keeping explicit filters and contexts other than row and column filters. 
        /// This function can be used to obtain visual totals in queries.
        /// This version generates a FILTER(ALLSELECTED(EXP) &amp;&amp; COND) form in DAX, that is fairly common
        /// </summary>
        /// <seealso cref="http://technet.microsoft.com/en-us/library/gg492186.aspx"/>
        /// <param name="exp">expression table or column reference</param>
        /// <param name="conditions">filters the table by these, conditions are translated</param>
        /// <typeparam name="T">expression type</typeparam> 
        /// <returns>boolean, the return value is never used, it is meant to be part of a filter expression in TabularTable Where()</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "exp", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "conditions", Justification = "Method is translated")]
        public static bool ForAllSelected<T>(this T exp, bool conditions)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// Create SUMX(table, expression) DAX Expression 
        /// Returns the sum of an expression evaluated for each row in a table.
        /// <seealso cref="http://technet.microsoft.com/en-us/library/ee634959.aspx" /> 
        /// </summary>
        /// <param name="table">The table containing the rows for which the expression will be evaluated.</param>
        /// <param name="projector">The expression to be evaluated for each row of the table.</param>
        /// <typeparam name="T">table element type</typeparam>
        /// <typeparam name="TValue">sum value type</typeparam>
        /// <returns>Sum of the selected values</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "projector", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "table", Justification = "Method is translated")]
        public static TValue? Sumx<T, TValue>(this IQueryable<T> table, Expression<Func<T, TValue>> projector)
            where TValue : struct
        {
            var provider = table.Provider as TabularQueryProvider;
            if (provider == null)
            {
                throw new NotImplementedException("Only available in a tabular query expression");
            }

            var m = (MethodInfo)MethodBase.GetCurrentMethod();
            var row = ProjectionExpression(typeof(TValue?), table, projector, AggregationType.Sum, m.MakeGenericMethod(new[] { typeof(T), typeof(TValue) }));
            var res = provider.Execute(row) as IEnumerable<TValue?>;
            if (res == null)
            {
                return null;
            }

            return res.FirstOrDefault();
        }

        /// <summary>
        /// Create SUMX(table, expression) DAX Expression 
        /// Returns the sum of an expression evaluated for each row in a table.
        /// <seealso cref="http://technet.microsoft.com/en-us/library/ee634959.aspx"/>
        /// </summary>
        /// <param name="table">
        /// The table containing the rows for which the expression will be evaluated.
        /// </param>
        /// <param name="projector">
        /// The expression to be evaluated for each row of the table.
        /// </param>
        /// <param name="token">
        /// The token.
        /// </param>
        /// <typeparam name="T">
        /// table element type
        /// </typeparam>
        /// <typeparam name="TValue">
        /// sum value type
        /// </typeparam>
        /// <returns>
        /// Sum of the selected values
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "projector", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "table", Justification = "Method is translated")]
        public static async Task<TValue?> SumxAsync<T, TValue>(this IQueryable<T> table, Expression<Func<T, TValue>> projector, CancellationToken token = default(CancellationToken))
            where TValue : struct
        {
            var provider = table.Provider as TabularQueryProvider;
            if (provider == null)
            {
                throw new NotImplementedException("Only available in a tabular query expression");
            }

            var m = (MethodInfo)MethodBase.GetCurrentMethod();
            var row = ProjectionExpression(typeof(TValue?), table, projector, AggregationType.Sum, m.MakeGenericMethod(new[] { typeof(T), typeof(TValue) }));
            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
            }

            var res = await provider.ExecuteAsync<IEnumerable<TValue?>>(row, token);
            if (res == null)
            {
                return null;
            }

            return res.FirstOrDefault();
        }

        /// <summary>
        /// Creates MAXX(table,expression) DAX expression
        /// Evaluates an expression for each row of a table and returns the largest numeric value.
        /// </summary>
        /// <seealso cref="http://technet.microsoft.com/en-us/library/ee634576.aspx"/>
        /// <param name="table">The table containing the rows for which the expression will be evaluated.</param>
        /// <param name="projector">The expression to be evaluated for each row of the table.</param>
        /// <typeparam name="T">type of query table elements</typeparam>
        /// <typeparam name="TValue">type of column and result</typeparam>
        /// <returns>the maximum value of the result of the projection on the table</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "projector", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "table", Justification = "Method is translated")]
        public static TValue? Maxx<T, TValue>(this IQueryable<T> table, Expression<Func<T, TValue>> projector)
            where TValue : struct
        {
            var provider = table.Provider as TabularQueryProvider;
            if (provider == null)
            {
                throw new NotImplementedException("Only available in a tabular query expression");
            }

            var m = (MethodInfo)MethodBase.GetCurrentMethod();
            var row = ProjectionExpression(typeof(TValue), table, projector, AggregationType.Max, m.MakeGenericMethod(new[] { typeof(T), typeof(TValue) }));
            var res = provider.Execute(row) as IEnumerable<TValue>;
            if (res == null)
            {
                return default(TValue);
            }

            return res.FirstOrDefault();
        }

        /// <summary>
        /// Creates MAXX(table,expression) DAX expression
        /// Evaluates an expression for each row of a table and returns the largest numeric value.
        /// </summary>
        /// <seealso cref="http://technet.microsoft.com/en-us/library/ee634576.aspx"/>
        /// <param name="table">
        /// The table containing the rows for which the expression will be evaluated.
        /// </param>
        /// <param name="projector">
        /// The expression to be evaluated for each row of the table.
        /// </param>
        /// <param name="token">
        /// The token.
        /// </param>
        /// <typeparam name="T">
        /// type of query table elements
        /// </typeparam>
        /// <typeparam name="TValue">
        /// type of column and result
        /// </typeparam>
        /// <returns>
        /// the maximum value of the result of the projection on the table
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "projector", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "table", Justification = "Method is translated")]
        public static async Task<TValue?> MaxxAsync<T, TValue>(this IQueryable<T> table, Expression<Func<T, TValue>> projector, CancellationToken token = default(CancellationToken))
            where TValue : struct
        {
            var provider = table.Provider as TabularQueryProvider;
            if (provider == null)
            {
                throw new NotImplementedException("Only available in a tabular query expression");
            }

            var m = (MethodInfo)MethodBase.GetCurrentMethod();
            var row = ProjectionExpression(typeof(TValue), table, projector, AggregationType.Max, m.MakeGenericMethod(new[] { typeof(T), typeof(TValue) }));
            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
            }

            var res = await provider.ExecuteAsync<IEnumerable<TValue>>(row, token);
            if (res == null)
            {
                return default(TValue);
            }

            return res.FirstOrDefault();
        }

        /// <summary>
        /// Create SUMX(table, expression) DAX Expression 
        /// Returns the sum of an expression evaluated for each row in a table.
        /// </summary>
        /// <seealso cref="http://technet.microsoft.com/en-us/library/ee634959.aspx"/>
        /// <param name="table">The table containing the rows for which the expression will be evaluated.</param>
        /// <param name="projector">The expression to be evaluated for each row of the table.</param>
        /// <typeparam name="T">table element type</typeparam>
        /// <typeparam name="TValue">result and column type</typeparam>
        /// <returns>sum of the column in the query</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
            "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "table", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "projector", Justification = "Method is translated")]
        public static TValue? Sumx<T, TValue>(this IQueryable<T> table, Expression<Func<T, TValue?>> projector)
            where TValue : struct
        {
            var provider = table.Provider as TabularQueryProvider;
            if (provider == null)
            {
                throw new NotImplementedException("Only available in a tabular query expression");
            }

            var m = (MethodInfo)MethodBase.GetCurrentMethod();
            var row = ProjectionExpression(typeof(TValue), table, projector, AggregationType.Sum, m.MakeGenericMethod(new[] { typeof(T), typeof(TValue) }));
            var res = provider.Execute(row) as IEnumerable<TValue>;
            if (res == null)
            {
                return null;
            }

            return res.FirstOrDefault();
        }

        /// <summary>
        /// Create SUMX(table, expression) DAX Expression 
        /// Returns the sum of an expression evaluated for each row in a table.
        /// </summary>
        /// <seealso cref="http://technet.microsoft.com/en-us/library/ee634959.aspx"/>
        /// <param name="table">
        /// The table containing the rows for which the expression will be evaluated.
        /// </param>
        /// <param name="projector">
        /// The expression to be evaluated for each row of the table.
        /// </param>
        /// <param name="token">
        /// The token.
        /// </param>
        /// <typeparam name="T">
        /// table element type
        /// </typeparam>
        /// <typeparam name="TValue">
        /// result and column type
        /// </typeparam>
        /// <returns>
        /// sum of the column in the query
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
            "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "table", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "projector", Justification = "Method is translated")]
        public static async Task<TValue?> SumxAsync<T, TValue>(this IQueryable<T> table, Expression<Func<T, TValue?>> projector, CancellationToken token = default(CancellationToken))
            where TValue : struct
        {
            var provider = table.Provider as TabularQueryProvider;
            if (provider == null)
            {
                throw new NotImplementedException("Only available in a tabular query expression");
            }

            var m = (MethodInfo)MethodBase.GetCurrentMethod();
            var row = ProjectionExpression(typeof(TValue), table, projector, AggregationType.Sum, m.MakeGenericMethod(new[] { typeof(T), typeof(TValue) }));
            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
            }

            var res = await provider.ExecuteAsync<IEnumerable<TValue>>(row, token);
            if (res == null)
            {
                return null;
            }

            return res.FirstOrDefault();
        }

        /// <summary>
        /// Creates MAXX(table,expression) DAX expression
        /// Evaluates an expression for each row of a table and returns the largest numeric value.
        /// </summary>
        /// <seealso cref="http://technet.microsoft.com/en-us/library/ee634576.aspx"/>
        /// <param name="table">The table containing the rows for which the expression will be evaluated.</param>
        /// <param name="projector">The expression to be evaluated for each row of the table.</param>
        /// <typeparam name="T">table element type</typeparam>
        /// <typeparam name="TValue">return and column type</typeparam>
        /// <returns>the maximum value of the result of the projection on the table</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
            "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "table", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "projector", Justification = "Method is translated")]
        public static TValue? Maxx<T, TValue>(this IQueryable<T> table, Expression<Func<T, TValue?>> projector)
            where TValue : struct
        {
            var provider = table.Provider as TabularQueryProvider;
            if (provider == null)
            {
                throw new NotImplementedException("Only available in a tabular query expression");
            }

            var m = (MethodInfo)MethodBase.GetCurrentMethod();
            var row = ProjectionExpression(typeof(TValue?), table, projector, AggregationType.Max, m.MakeGenericMethod(new[] { typeof(T), typeof(TValue) }));
            var res = provider.Execute(row) as IEnumerable<TValue?>;
            if (res == null)
            {
                return null;
            }

            return res.FirstOrDefault();
        }

        /// <summary>
        /// Creates MAXX(table,expression) DAX expression
        /// Evaluates an expression for each row of a table and returns the largest numeric value.
        /// </summary>
        /// <seealso cref="http://technet.microsoft.com/en-us/library/ee634576.aspx"/>
        /// <param name="table">
        /// The table containing the rows for which the expression will be evaluated.
        /// </param>
        /// <param name="projector">
        /// The expression to be evaluated for each row of the table.
        /// </param>
        /// <param name="token">
        /// The token.
        /// </param>
        /// <typeparam name="T">
        /// table element type
        /// </typeparam>
        /// <typeparam name="TValue">
        /// return and column type
        /// </typeparam>
        /// <returns>
        /// the maximum value of the result of the projection on the table
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
            "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "table", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "projector", Justification = "Method is translated")]
        public static async Task<TValue?> MaxxAsync<T, TValue>(this IQueryable<T> table, Expression<Func<T, TValue?>> projector, CancellationToken token = default(CancellationToken))
            where TValue : struct
        {
            var provider = table.Provider as TabularQueryProvider;
            if (provider == null)
            {
                throw new NotImplementedException("Only available in a tabular query expression");
            }

            var m = (MethodInfo)MethodBase.GetCurrentMethod();
            var row = ProjectionExpression(typeof(TValue?), table, projector, AggregationType.Max, m.MakeGenericMethod(new[] { typeof(T), typeof(TValue) }));
            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
            }

            var res = await provider.ExecuteAsync<IEnumerable<TValue?>>(row, token);
            if (res == null)
            {
                return null;
            }

            return res.FirstOrDefault();
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "projector", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "table", Justification = "Method is translated")]
        public static TValue? Minx<T, TValue>(this IQueryable<T> table, Expression<Func<T, TValue>> projector)
            where TValue : struct
        {
            var provider = table.Provider as TabularQueryProvider;
            if (provider == null)
            {
                throw new NotImplementedException("Only available in a tabular query expression");
            }

            var m = (MethodInfo)MethodBase.GetCurrentMethod();
            var row = ProjectionExpression(typeof(TValue), table, projector, AggregationType.Min, m.MakeGenericMethod(new[] { typeof(T), typeof(TValue) }));
            var res = provider.Execute(row) as IEnumerable<TValue>;
            if (res == null)
            {
                return default(TValue);
            }

            return res.FirstOrDefault();
        }

        /// <summary>
        /// Creates MINX(table,expression) DAX expression
        /// Returns the smallest numeric value that results from evaluating an expression for each row of a table.
        /// <see cref="http://technet.microsoft.com/en-us/library/ee634545.aspx"></see>
        /// </summary>
        /// <param name="table">
        /// The table containing the rows for which the expression will be evaluated.
        /// </param>
        /// <param name="projector">
        /// The expression to be evaluated for each row of the table.
        /// </param>
        /// <param name="token">
        /// The token.
        /// </param>
        /// <returns>
        /// the minimum value of the result of the projection on the table
        /// </returns>
        /// <typeparam name="T">
        /// type of the table
        /// </typeparam>
        /// <typeparam name="TValue">
        /// type of the value returned
        /// </typeparam>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "projector", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "table", Justification = "Method is translated")]
        public static async Task<TValue?> MinxAsync<T, TValue>(this IQueryable<T> table, Expression<Func<T, TValue>> projector, CancellationToken token = default(CancellationToken))
            where TValue : struct
        {
            var provider = table.Provider as TabularQueryProvider;
            if (provider == null)
            {
                throw new NotImplementedException("Only available in a tabular query expression");
            }

            var m = (MethodInfo)MethodBase.GetCurrentMethod();
            var row = ProjectionExpression(typeof(TValue), table, projector, AggregationType.Min, m.MakeGenericMethod(new[] { typeof(T), typeof(TValue) }));
            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
            }

            var res = await provider.ExecuteAsync<IEnumerable<TValue>>(row, token);
            if (res == null)
            {
                return default(TValue);
            }

            return res.FirstOrDefault();
        }

        /// <summary>
        /// Creates MINX(table,expression) DAX expression
        /// Returns the smallest numeric value that results from evaluating an expression for each row of a table.
        /// </summary>
        /// <seealso cref="http://technet.microsoft.com/en-us/library/ee634545.aspx"/>
        /// <param name="table">The table containing the rows for which the expression will be evaluated.</param>
        /// <param name="projector">The expression to be evaluated for each row of the table.</param>
        /// <typeparam name="T">Table element type</typeparam>
        /// <typeparam name="TValue">column and return type</typeparam>
        /// <returns>the minimum value of the result of the projection on the table</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
            "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "projector", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "table", Justification = "Method is translated")]
        public static TValue? Minx<T, TValue>(this IQueryable<T> table, Expression<Func<T, TValue?>> projector)
            where TValue : struct
        {
            var provider = table.Provider as TabularQueryProvider;
            if (provider == null)
            {
                throw new NotImplementedException("Only available in a tabular query expression");
            }

            var m = (MethodInfo)MethodBase.GetCurrentMethod();
            var row = ProjectionExpression(typeof(TValue?), table, projector, AggregationType.Min, m.MakeGenericMethod(new[] { typeof(T), typeof(TValue) }));
            var res = provider.Execute(row) as IEnumerable<TValue?>;
            if (res == null)
            {
                return null;
            }

            return res.FirstOrDefault();
        }

        /// <summary>
        /// Creates MINX(table,expression) DAX expression
        /// Returns the smallest numeric value that results from evaluating an expression for each row of a table.
        /// </summary>
        /// <seealso cref="http://technet.microsoft.com/en-us/library/ee634545.aspx"/>
        /// <param name="table">
        /// The table containing the rows for which the expression will be evaluated.
        /// </param>
        /// <param name="projector">
        /// The expression to be evaluated for each row of the table.
        /// </param>
        /// <param name="token">
        /// The token.
        /// </param>
        /// <typeparam name="T">
        /// Table element type
        /// </typeparam>
        /// <typeparam name="TValue">
        /// column and return type
        /// </typeparam>
        /// <returns>
        /// the minimum value of the result of the projection on the table
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
            "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "projector", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "table", Justification = "Method is translated")]
        public static async Task<TValue?> MinxAsync<T, TValue>(this IQueryable<T> table, Expression<Func<T, TValue?>> projector, CancellationToken token = default(CancellationToken))
            where TValue : struct
        {
            var provider = table.Provider as TabularQueryProvider;
            if (provider == null)
            {
                throw new NotImplementedException("Only available in a tabular query expression");
            }

            var m = (MethodInfo)MethodBase.GetCurrentMethod();
            var row = ProjectionExpression(typeof(TValue?), table, projector, AggregationType.Min, m.MakeGenericMethod(new[] { typeof(T), typeof(TValue) }));
            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
            }

            var res = await provider.ExecuteAsync<IEnumerable<TValue?>>(row, token);
            if (res == null)
            {
                return null;
            }

            return res.FirstOrDefault();
        }

        /// <summary>
        /// Creates a RANKX(table, expression) DAX expression (value, order and ties arguments are not supported yet)
        /// Returns the ranking of a number in a list of numbers for each row in the table argument.
        /// </summary>
        /// <seealso cref="http://technet.microsoft.com/en-us/library/gg492185.aspx"/>
        /// <param name="table">DAX expression that returns a table of data over which the expression is evaluated..</param>
        /// <param name="projector">The expression to be evaluated for each row of the table.</param>
        /// <typeparam name="T">query element type</typeparam>
        /// <typeparam name="TValue">column and return type</typeparam> 
        /// <returns>ranking of a number </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "projector", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "table", Justification = "Method is translated")]
        public static long? Rankx<T, TValue>(this IQueryable<T> table, Expression<Func<T, TValue>> projector)
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// Shortcut for reverse ranking by taking -expression as ranking,
        /// (this function will be removed, when ran order will be supported in RANKX)
        /// Creates a RANKX(table, -expression) DAX expression (value, order and ties arguments are not supported yet)
        /// Returns the ranking of a number in a list of numbers for each row in the table argument. 
        /// </summary>
        /// <seealso cref="http://technet.microsoft.com/en-us/library/gg492185.aspx"/>
        /// <param name="table">DAX expression that returns a table of data over which the expression is evaluated..</param>
        /// <param name="projector">The expression to be evaluated for each row of the table.</param>
        /// <typeparam name="T">table element type</typeparam>
        /// <typeparam name="TValue">column type</typeparam>
        /// <returns>ranking of a number </returns> 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "projector", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "table", Justification = "Method is translated")]
        public static long? ReverseRankx<T, TValue>(this IQueryable<T> table, Expression<Func<T, TValue>> projector)
            where TValue : struct
        {
            throw new NotImplementedException("Only available in a tabular query expression");
        }

        /// <summary>
        /// AVERAGEX function call.
        /// </summary>
        /// <typeparam name="T">type of table rows</typeparam>
        /// <typeparam name="TValue">type of result</typeparam>
        /// <param name="table">table query</param>
        /// <param name="projector">projection to a column</param>
        /// <returns>average value of the column in the query</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "projector", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "table", Justification = "Method is translated")]
        public static TValue? Averagex<T, TValue>(this IQueryable<T> table, Expression<Func<T, TValue>> projector)
            where TValue : struct
        {
            var provider = table.Provider as TabularQueryProvider;
            if (provider == null)
            {
                throw new NotImplementedException("Only available in a tabular query expression");
            }

            var m = (MethodInfo)MethodBase.GetCurrentMethod();
            var row = ProjectionExpression(typeof(TValue), table, projector, AggregationType.Avg, m.MakeGenericMethod(new[] { typeof(T), typeof(TValue) }));
            var res = provider.Execute(row) as IEnumerable<TValue?>;
            if (res == null)
            {
                return null;
            }

            return res.FirstOrDefault();
        }

        /// <summary>
        /// AVERAGEX function call.
        /// </summary>
        /// <typeparam name="T">
        /// type of table rows
        /// </typeparam>
        /// <typeparam name="TValue">
        /// type of result
        /// </typeparam>
        /// <param name="table">
        /// table query
        /// </param>
        /// <param name="projector">
        /// projection to a column
        /// </param>
        /// <param name="token">
        /// The token.
        /// </param>
        /// <returns>
        /// average value of the column in the query
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "projector", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "table", Justification = "Method is translated")]
        public static async Task<TValue?> AveragexAsync<T, TValue>(this IQueryable<T> table, Expression<Func<T, TValue>> projector, CancellationToken token = default(CancellationToken))
            where TValue : struct
        {
            var provider = table.Provider as TabularQueryProvider;
            if (provider == null)
            {
                throw new NotImplementedException("Only available in a tabular query expression");
            }

            var m = (MethodInfo)MethodBase.GetCurrentMethod();
            var row = ProjectionExpression(typeof(TValue), table, projector, AggregationType.Avg, m.MakeGenericMethod(new[] { typeof(T), typeof(TValue) }));
            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
            }

            var res = await provider.ExecuteAsync<IEnumerable<TValue?>>(row, token);
            if (res == null)
            {
                return null;
            }

            return res.FirstOrDefault();
        }

        /// <summary>
        /// COUNTX function call.
        /// </summary>
        /// <typeparam name="T">type of the table</typeparam>
        /// <typeparam name="TValue">type of the value counted</typeparam>
        /// <param name="table">Tabular table or table expression</param>
        /// <param name="projector">function to select the value to count, must be column reference</param>
        /// <returns>count of values</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "projector", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "table", Justification = "Method is translated")]
        public static long? Countx<T, TValue>(this IQueryable<T> table, Expression<Func<T, TValue>> projector)
        {
            var provider = table.Provider as TabularQueryProvider;
            if (provider == null)
            {
                throw new NotImplementedException("Only available in a tabular query expression");
            }

            var m = (MethodInfo)MethodBase.GetCurrentMethod();
            var row = ProjectionExpression(typeof(long?), table, projector, AggregationType.Count, m.MakeGenericMethod(new[] { typeof(T), typeof(TValue) }));
            var res = provider.Execute(row) as IEnumerable<long?>;
            if (res == null)
            {
                return null;
            }

            return res.FirstOrDefault();
        }

        /// <summary>
        /// COUNTX function call.
        /// </summary>
        /// <typeparam name="T">
        /// type of the table
        /// </typeparam>
        /// <typeparam name="TValue">
        /// type of the value counted
        /// </typeparam>
        /// <param name="table">
        /// Tabular table or table expression
        /// </param>
        /// <param name="projector">
        /// function to select the value to count, must be column reference
        /// </param>
        /// <param name="token">
        /// The token.
        /// </param>
        /// <returns>
        /// count of values
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "projector", Justification = "Method is translated"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "table", Justification = "Method is translated")]
        public static async Task<long?> CountxAsync<T, TValue>(this IQueryable<T> table, Expression<Func<T, TValue>> projector, CancellationToken token = default(CancellationToken))
        {
            var provider = table.Provider as TabularQueryProvider;
            if (provider == null)
            {
                throw new NotImplementedException("Only available in a tabular query expression");
            }

            var m = (MethodInfo)MethodBase.GetCurrentMethod();
            var row = ProjectionExpression(typeof(long?), table, projector, AggregationType.Count, m.MakeGenericMethod(new[] { typeof(T), typeof(TValue) }));
            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
            }

            var res = await provider.ExecuteAsync<IEnumerable<long?>>(row, token);
            if (res == null)
            {
                return null;
            }

            return res.FirstOrDefault();
        }

        /// <summary>
        /// COUNTROWS function
        /// </summary>
        /// <typeparam name="T">Type of tabular table</typeparam>
        /// <param name="table">table to be counted</param>
        /// <returns>number of rows</returns>
        public static long? CountRows<T>(this IQueryable<T> table)
        {
            var provider = table.Provider as TabularQueryProvider;
            if (provider == null)
            {
                throw new NotImplementedException("Only available in a tabular query expression");
            }

            var binder = new TabularQueryBinder();
            var e = TabularEvaluator.PartialEval(table.Expression);
            var t = (ProjectionExpression)binder.Bind(e);
            string tableName = ColumnExpressionFactory.FindTableName(table.Expression);
            var type = typeof(long?);
            var aggregation = new XAggregationExpression(AggregationType.CountRows, t, null, "[" + "Count" + "]", tableName, type);
            var measure = new MeasureDeclaration("Count", "Count", aggregation);
            var row = new ProjectionExpression(
                new RowExpression(type, measure), measure.Expression);
            var res = provider.Execute(row) as IEnumerable<long?>;
            if (res == null)
            {
                return null;
            }

            return res.FirstOrDefault();
        }

        /// <summary>
        /// COUNTROWS function asynchronously
        /// </summary>
        /// <typeparam name="T">Type of tabular table</typeparam>
        /// <param name="table">table to be counted</param>
        /// <param name="token">cancellation token</param>
        /// <returns>number of rows</returns>
        public static async Task<long?> CountRowsAsync<T>(this IQueryable<T> table, CancellationToken token = default(CancellationToken))
        {
            var provider = table.Provider as TabularQueryProvider;
            if (provider == null)
            {
                throw new NotImplementedException("Only available in a tabular query expression");
            }

            var binder = new TabularQueryBinder();
            var e = TabularEvaluator.PartialEval(table.Expression);
            var t = (ProjectionExpression)binder.Bind(e);
            string tableName = ColumnExpressionFactory.FindTableName(table.Expression);
            var type = typeof(long?);
            var aggregation = new XAggregationExpression(AggregationType.CountRows, t, null, "[" + "Count" + "]", tableName, type);
            var measure = new MeasureDeclaration("Count", "Count", aggregation);
            var row = new ProjectionExpression(
                new RowExpression(type, measure), measure.Expression);
            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
            }

            var res = await provider.ExecuteAsync<IEnumerable<long?>>(row, token);
            if (res == null)
            {
                return null;
            }

            return res.FirstOrDefault();
        }

        /// <summary>
        /// Execute to list asynchronously
        /// </summary>
        /// <typeparam name="TResult">result type</typeparam>
        /// <param name="query">query to execute</param>
        /// <param name="token">cancellation token</param>
        /// <returns>a task of list of results</returns>
        public static async Task<List<TResult>> ToListAsync<TResult>(this IQueryable<TResult> query, CancellationToken token = default(CancellationToken))
        {
            var provider = query.Provider as TabularQueryProvider;
            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();   
            }

            if (provider != null)
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }

                var ie = await provider.ExecuteAsync<IEnumerable<TResult>>(query.Expression, token);
                return ie.ToList();
            }

            return await Task.Factory.StartNew(() => query.ToList(), token);
        }

        /// <summary>
        /// Execute to array asynchronously
        /// </summary>
        /// <typeparam name="TResult">result type</typeparam>
        /// <param name="query">query to execute</param>
        /// <param name="token">cancellation token</param>
        /// <returns>a task of list of results</returns>
        public static async Task<TResult[]> ToArrayAsync<TResult>(this IQueryable<TResult> query, CancellationToken token = default(CancellationToken))
        {
            var provider = query.Provider as TabularQueryProvider;
            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
            }

            if (provider != null)
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }

                var ie = await provider.ExecuteAsync<IEnumerable<TResult>>(query.Expression, token);
                return ie.ToArray();
            }

            return await Task.Factory.StartNew(() => query.ToArray(), token);
        }

        private static ProjectionExpression ProjectionExpression<T, TValue>(Type returnType, IQueryable<T> table, Expression<Func<T, TValue>> projector, AggregationType aggregationType, MethodInfo method)
        {
            var binder = new TabularQueryBinder();
            var e = TabularEvaluator.PartialEval(table.Expression);
            var methodCall = Expression.Call(null, method, new[] { e, projector });
            var type = returnType;
            var aggregation = new ColumnExpressionFactory(binder).CreateXAggregation(aggregationType, methodCall);
            var measure = new MeasureDeclaration("Result", "Result", aggregation);
            var row = new ProjectionExpression(
                new RowExpression(type, measure), measure.Expression);
            return row;
        }
    }
}