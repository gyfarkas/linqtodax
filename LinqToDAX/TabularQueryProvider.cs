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

        public TabularQueryProvider(string connectionString)
        {
            Connection = new OleDbConnection(connectionString);
        }

        private OleDbConnection Connection { get; set; }

        public event Logger Log; 
        
        public static string GetQueryText(Expression expression)
        {
            return Translate(expression).CommandText;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TabularTable<TElement>(this, expression);
        }

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

        public object Execute(Expression expression)
        {
            TranslateResult result = Translate(expression);
            Delegate projector = result.Projector.Compile();
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }

            OleDbCommand cmd = Connection.CreateCommand();
            cmd.CommandText = result.CommandText;
            Log.Invoke(result.CommandText);
            OleDbDataReader reader = cmd.ExecuteReader();

            Type elementType = TypeSystem.GetElementType(expression.Type);
            return Activator.CreateInstance(
                typeof(ProjectionReader<>).MakeGenericType(elementType),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[] { reader, projector },
                null);
        }

        public TResult Execute<TResult>(Expression expression)
        {
           return (TResult)this.Execute(expression); 
        }

        private static TranslateResult Translate(Expression expression)
        {
            expression = TabularEvaluator.PartialEval(expression);
            var projection = (ProjectionExpression)new TabularQueryBinder().Bind(expression);
            string commandText = new TabularQueryFormatter().Format(projection.Source);
            
            LambdaExpression projector = new ProjectionBuilder().Build(projection.Projector);
            return 
                new TranslateResult 
                {
                    CommandText = commandText, 
                    Projector = projector
                };
        }

        private class TranslateResult
        {
            internal string CommandText { get; set; }
            internal LambdaExpression Projector { get; set; }
        }
    }
}