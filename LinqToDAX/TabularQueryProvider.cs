// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TabularQueryProvider.cs" company="Dealogic">
//   LICENCE.md
// </copyright>
// <summary>
//   This provider version uses expressions as internal representation of queries throughout
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LinqToDAX
{
    using System;
    using System.Data;
    using System.Data.OleDb;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Query;
    using Query.DAXExpression;
    using QueryFormatter;

    /// <summary>
    /// Logging function
    /// </summary>
    /// <param name="message">log message</param>
    public delegate void Logger(string message);

    /// <summary>
    /// This provider version uses expressions as internal representation of queries throughout
    /// </summary>
    public class TabularQueryProvider : IQueryProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TabularQueryProvider"/> class.
        /// </summary>
        /// <param name="connectionString">connection string for the tabular database</param>
        public TabularQueryProvider(string connectionString)
        {
            Connection = new OleDbConnection(connectionString);
        }

        /// <summary>
        /// Delegate to log the queries
        /// </summary>
        public event Logger Log; 

        private OleDbConnection Connection { get; set; }

        /// <summary>
        /// Gets the query text from the translation
        /// </summary>
        /// <param name="expression">expression to be translated</param>
        /// <returns>query string</returns>
        public string GetQueryText(Expression expression)
        {
            return Translate(expression).CommandText;
        }

        /// <summary>
        /// Create executable query from expression
        /// </summary>
        /// <typeparam name="TElement">Element type</typeparam>
        /// <param name="expression">expression to evaluate</param>
        /// <returns>result object</returns>
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TabularTable<TElement>(this, expression);
        }

        /// <summary>
        /// Non strictly typed version for creating queries
        /// </summary>
        /// <param name="expression">expression to transform</param>
        /// <returns>query object</returns>
        public IQueryable CreateQuery(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);
            try
            {
                return
                    (IQueryable)
                        Activator.CreateInstance(
                        typeof(TabularTable<>).MakeGenericType(elementType),
                        new object[] { this, expression });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        /// <summary>
        /// Non strictly typed version for executing queries
        /// </summary>
        /// <param name="expression">expression to be executed</param>
        /// <returns>result object</returns>
        public object Execute(Expression expression)
        {
            return this.Execute(Translate(expression)); 
        }

        /// <summary>
        /// Executes the query
        /// </summary>
        /// <typeparam name="TResult">type of the result</typeparam>
        /// <param name="expression">expression to be translated and executed on SSAS</param>
        /// <returns>the result of the query</returns>
        public TResult Execute<TResult>(Expression expression)
        {
           return (TResult)this.Execute(Translate(expression)); 
        }

        /// <summary>
        /// Asynchronous command execution 
        /// </summary>
        /// <typeparam name="TResult">type of the result</typeparam>
        /// <param name="expression">expression to be executed on SSAS</param>
        /// <param name="token">cancellation token</param>
        /// <returns>Task result</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "There is no SQL here")]
        public async Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken token)
        {
            var translateResult = Translate(expression);
            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
            }

            if (Connection.State != ConnectionState.Open)
            {
                await Connection.OpenAsync(token);
            }

            var cmd = Connection.CreateCommand();
            cmd.CommandText = translateResult.CommandText;
            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
            }

            var reader = await cmd.ExecuteReaderAsync(token);

            Type elementType = TypeSystem.GetElementType(translateResult.Projector.Body.Type);

            var res = Activator.CreateInstance(
                typeof(ProjectionReader<>).MakeGenericType(elementType),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[] { reader, translateResult.Projector.Compile(), this },
                null);
            return (TResult)res;
        }

        private TranslateResult Translate(Expression expression)
        {
            var projection = expression as ProjectionExpression;
            var subQuery = expression as SubQueryProjection;
            
            if (subQuery != null)
            {
                projection = subQuery.Projection;
            }
            expression = TabularEvaluator.PartialEval(expression);
            var boundExpression = new TabularQueryBinder(this).Bind(expression);

            if (projection == null && subQuery == null)
            {
               return Translate(boundExpression);
            }

            if (projection == null)
            {
                
                throw new TabularException("Could not translate");
            }

            string commandText = new TabularQueryFormatter().Format(projection.Source);
            
            LambdaExpression projector = new ProjectionBuilder().Build(projection.Projector);
            return 
                new TranslateResult 
                {
                    CommandText = commandText, 
                    Projector = projector
                };
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "There is no SQL here")]
        private object Execute(TranslateResult result)
        {
            Delegate projector = result.Projector.Compile();
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }

            OleDbCommand cmd = Connection.CreateCommand();
            cmd.CommandText = result.CommandText;

            if (Log != null)
            {
                Log.Invoke(result.CommandText);
            }

            OleDbDataReader reader = cmd.ExecuteReader();

            Type elementType = result.Projector.Body.Type; //TypeSystem.GetElementType(result.Projector.Body.Type);

            return Activator.CreateInstance(
                typeof(ProjectionReader<>).MakeGenericType(elementType),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[] { reader, projector, this },
                null);
        }

        private class TranslateResult
        {
            internal string CommandText { get; set; }

            internal LambdaExpression Projector { get; set; }
        }
    }
}