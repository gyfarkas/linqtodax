// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TableFactory.cs" company="Dealogic">
//   See LICENCE.md
// </copyright>
// <summary>
//   Class responsible for creating table expressions and references
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LinqToDAX.Query.DAXExpression
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using TabularEntities;

    /// <summary>
    /// Class responsible for creating table expressions and references 
    /// </summary>
    internal class TableFactory
    {

        /// <summary>
        /// Reference to the binder object
        /// </summary>
        private readonly TabularQueryBinder _binder;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableFactory"/> class.
        /// </summary>
        /// <param name="binder">reference for the binder object</param>
        public TableFactory(TabularQueryBinder binder)
        {
            _binder = binder;
        }

        /// <summary>
        /// Find table expression in node 
        /// </summary>
        /// <param name="node">expression that should be a table reference</param>
        /// <returns>Table expression</returns>
        internal static Expression GetTableExpression(Expression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return TableExpressionFromMember((MemberExpression)node);
                case ExpressionType.Parameter:
                    return TableExpressionFromParameter((ParameterExpression)node);
                case ExpressionType.Constant:
                    return TableExpressionFromConstant((ConstantExpression) node);
                case ExpressionType.MemberInit:
                    return TableExpressionFromMemberInit((MemberInitExpression)node);
                case ExpressionType.New:
                    return TableExpressionFromNew((NewExpression)node);
                default:
                    return node;
            }
        }


        internal static Expression TableExpressionFromConstant(ConstantExpression node)
        {
            var mapping = node.Type.GetCustomAttribute(typeof(TabularTableMappingAttribute)) as
                    TabularTableMappingAttribute;
            if (mapping != null)
            {
                string dbname = mapping.TableName;
                return new TableExpression(node.Type, dbname);
            }

            throw new ArgumentException("Should have been a type representing a table");
        }

        /// <summary>
        /// Extracts the table name form an <see cref="IQueryable"/>
        /// </summary>
        /// <param name="value"> <see cref="IQueryable"/> that has a table mapping attribute</param>
        /// <returns>table name</returns>
        internal static string GetTableName(IQueryable value)
        {
            var tableMapping =
                value.GetType().GetGenericArguments()[0]
                    .GetCustomAttribute(typeof(TabularTableMappingAttribute))
                    as TabularTableMappingAttribute;
            if (tableMapping != null)
            {
                return tableMapping.TableName;
            }

            Type rowType = value.ElementType;
            return rowType.Name;
        }

        /// <summary>
        /// Finds the table that a parameter expression refers to
        /// </summary>
        /// <param name="node">parameter expression</param>
        /// <returns>Table expression</returns>
        internal static Expression TableExpressionFromParameter(ParameterExpression node)
        {
            var mapping =
                node.Type.GetCustomAttribute(typeof(TabularTableMappingAttribute)) as
                    TabularTableMappingAttribute;
            if (mapping != null)
            {
                string dbname = mapping.TableName;
                return new TableExpression(node.Type, dbname);
            }

            throw new ArgumentException("Should have been a type representing a table");
        }

        /// <summary>
        /// finds the table that a member refers to, used for related tables
        /// </summary>
        /// <param name="m">member expression</param>
        /// <returns>table expression</returns>
        internal static Expression TableExpressionFromMember(MemberExpression m)
        {
            if (m.Member.MemberType == MemberTypes.Property)
            {
                var pi = (PropertyInfo) m.Member;

                var mapping =
                    pi.PropertyType.GetCustomAttribute(typeof(TabularTableMappingAttribute)) as
                        TabularTableMappingAttribute;
                if (mapping != null)
                {
                    string dbname = mapping.TableName;
                    return new TableExpression(m.Member.GetType(), dbname);
                }
            }
            return m;
        }

        internal static Expression TableExpressionFromNew(NewExpression n)
        {
            var mapping =
                n.Type.GetCustomAttribute(typeof (TabularTableMappingAttribute)) as TabularTableMappingAttribute;
            if (mapping != null)
            {
                string dbname = mapping.TableName;
                return new TableExpression(n.Type, dbname);
            }

            return n;
        }

        internal static Expression TableExpressionFromMemberInit(MemberInitExpression init)
        {
            var mapping =
                init.Type.GetCustomAttribute(typeof (TabularTableMappingAttribute)) as TabularTableMappingAttribute;
            if (mapping != null)
            {
                string dbname = mapping.TableName;
                return new TableExpression(init.Type, dbname);
            }

            return init;
        }

        /// <summary>
        /// creates a projection expression from an <see cref="IQueryable"/> object 
        /// </summary>
        /// <param name="table"><see cref="IQueryable"/>object</param>
        /// <returns>projection expression for the table</returns>
        internal ProjectionExpression GetTableProjection(IQueryable table)
        {
            var bindings = new List<MemberBinding>();
            Type t = table.ElementType;
            foreach (PropertyInfo mi in t.GetProperties())
            {
                string columnName = mi.Name;
                Type columnType = ColumnExpressionFactory.GetColumnType(mi);
                if (typeof(ITabularData).IsAssignableFrom(columnType))
                {
                    object c = Activator.CreateInstance(columnType); 
                    Expression relatedTable = Expression.Constant(c);
                    bindings.Add(Expression.Bind(mi, relatedTable));
                }
                else
                {
                    var mapping = mi.GetCustomAttribute(typeof(TabularMappingAttribute)) as TabularMappingAttribute;
                    if (mapping != null)
                    {
                        string dbname = mapping.ColumnName;
                        string tableName = mapping.TableName;
                        var ce = new ColumnExpression(columnType, columnName, dbname, tableName);
                        bindings.Add(Expression.Bind(mi, ce));
                    }
                }
            }

            Expression projector = Expression.MemberInit(Expression.New(t), bindings);
            Type resultType = typeof(IEnumerable<>).MakeGenericType(t);
            return
                new ProjectionExpression(
                    new TableExpression(
                        resultType,
                        GetTableName(table)),
                    projector);
        }
    }
}