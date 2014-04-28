using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LinqToDAX
{
    /// <summary>
    ///     IQueryable implementation
    ///     Test with:
    ///     You see the IQueryable contains an expression that represents a snippet of code that
    ///     if turned into actual code and executed would reconstruct that very same IQueryable (or its equivalent).
    /// </summary>
    public class TabularTable<TData> : IQueryable<TData>
    {
        private readonly Expression _expression;

        private readonly TabularQueryProvider _provider;

        public TabularTable(TabularQueryProvider provider)
        {
            _provider = provider;
            _expression = Expression.Constant(this);
        }

        public TabularTable(TabularQueryProvider provider, Expression expression)
        {
            if (provider == null)
                throw new ArgumentNullException("provider", "No provider was given to Tabular report constructor");

            if (expression == null)
                throw new ArgumentNullException("expression", "No expression was given to Tabular report constructor");

            if (!typeof (IQueryable<TData>).IsAssignableFrom(expression.Type))
                throw new ArgumentException("Invalid type was given to Tabular table constructor");

            _provider = provider;
            _expression = expression;
        }

        public Type ElementType
        {
            get { return typeof (TData); }
        }

        public Expression Expression
        {
            get { return _expression; }
        }

        public IQueryProvider Provider
        {
            get { return _provider; }
        }

        public IEnumerator GetEnumerator()
        {
            return (Provider.Execute<IEnumerable>(Expression)).GetEnumerator();
        }

        IEnumerator<TData> IEnumerable<TData>.GetEnumerator()
        {
            return (Provider.Execute<IEnumerable<TData>>(Expression)).GetEnumerator();
        }
    }
}