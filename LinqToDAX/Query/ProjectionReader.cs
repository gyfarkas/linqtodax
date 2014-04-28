// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectionReader.cs" company="Dealogic">
//   see LICENCE.md
// </copyright>
// <summary>
//   Defines the ProjectionReader type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LinqToDAX.Query
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Common;

    /// <summary>
    /// Reads the result of the query with the help of the projector function built during the query binding/building
    /// </summary>
    /// <typeparam name="T">The runtime type of the expression that was built together with the projector function</typeparam>
    internal class ProjectionReader<T> : IEnumerable<T>, IDisposable
    {
        /// <summary>
        /// Enumerator for the reader, the type is defined in this class
        /// </summary>
        private Enumerator _enumerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectionReader{T}"/> class.
        /// </summary>
        /// <param name="reader">DataReader that contains the results of the query</param>
        /// <param name="projector">projector function that was built with the query</param>
        internal ProjectionReader(DbDataReader reader, Func<ProjectionRow, T> projector)
        {
            _enumerator = new Enumerator(reader, projector);
        }

        /// <summary>
        /// disposes the resources
        /// </summary>
        void IDisposable.Dispose()
        {
            _enumerator.Dispose();
        }

        /// <summary>
        /// returns the enumerator
        /// </summary>
        /// <returns><see cref="IEnumerator{T}"/> enumerator</returns>
        public IEnumerator<T> GetEnumerator()
        {
            Enumerator e = _enumerator;
            if (e == null)
            {
                throw new InvalidOperationException("cannot enumerate more then once");
            }
            _enumerator = null;
            return e;
        }

        /// <summary>
        /// returns an enumerator
        /// </summary>
        /// <returns><see cref="IEnumerator"/> enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// this internal class creates the enumeration of rows 
        /// </summary>
        private class Enumerator : ProjectionRow, IEnumerator<T>
        {
            /// <summary>
            /// projector function from the query projection expression
            /// </summary>
            private readonly Func<ProjectionRow, T> _projector;

            /// <summary>
            /// Data Reader holding the data form the executed query
            /// </summary>
            private readonly DbDataReader _reader;

            /// <summary>
            /// Initializes a new instance of the <see cref="Enumerator"/> class. 
            /// </summary>
            /// <param name="reader">data reader of the results</param>
            /// <param name="projector">projector function</param>
            internal Enumerator(DbDataReader reader, Func<ProjectionRow, T> projector)
            {
                _reader = reader;
                _projector = projector;
            }

            /// <summary>
            /// Gets the current value of the enumeration
            /// </summary>
            public T Current { get; private set; }

            /// <summary>
            /// Gets <see cref="IEnumerator"/> Current value
            /// </summary>
            object IEnumerator.Current
            {
                get { return Current; }
            }

            /// <summary>
            /// Steps with the enumeration
            /// </summary>
            /// <returns>boolean value expressing whether the the step was successful</returns>
            public bool MoveNext()
            {
                if (_reader.Read())
                {
                    Current = _projector(this);

                    return true;
                }
                return false;
            }

            /// <summary>
            /// Empty implementation of reset
            /// </summary>
            public void Reset()
            {
            }

            /// <summary>
            /// Disposes the resources, the reader
            /// </summary>
            public void Dispose()
            {
                _reader.Dispose();
            }

            /// <summary>
            /// Gets a value form the enumeration
            /// </summary>
            /// <param name="index">column name</param>
            /// <param name="t">runtime type of the column to be read</param>
            /// <returns>an object from the column</returns>
            public override object GetValue(string index, Type t)
            {
                if (_reader[index] == null || _reader[index] == DBNull.Value)
                {
                    if (t == typeof(int))
                    {
                        return 0;
                    }
                    
                    // create a default value for the field;
                    if (t == typeof(string))
                    {
                        return string.Empty;
                    }

                    return Activator.CreateInstance(t);
                }

                if (t == typeof(string))
                {
                    return _reader[index].ToString();
                }

                return _reader[index];
            }
        }
    }
}