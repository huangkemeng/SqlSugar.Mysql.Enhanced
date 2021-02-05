using KeMengSoft.AttrReflector;
using SqlSugar;
using Sqsugar.Mysql.Enhanced.Enums;
using Sqsugar.Mysql.Enhanced.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Sqsugar.Mysql.Enhanced.Models
{
    public class MtInsertableProvider<T>
    {
        private readonly string _tableName;
        private List<DbColumnInfo> _dbColumnInfos;
        private QueryBuilder _queryBuilder;
        public MtInsertableProvider()
        {
            Type t = typeof(T);
            _tableName = t.Name;
            SugarTable sugarTable = t.GetAttributeInfo<SugarTable>();
            if (sugarTable != null)
            {
                if (!string.IsNullOrWhiteSpace(sugarTable.TableName))
                {
                    _tableName = sugarTable.TableName;
                }
            }
            _dbColumnInfos = new List<DbColumnInfo>();
            _dbColumnInfos.ConcatWith(t.GetProperties(), prop =>
            {
                SugarColumn sugarColumn = prop.GetAttributeInfo<SugarColumn>();
                if (sugarColumn != null)
                {
                    return !sugarColumn.IsOnlyIgnoreInsert;
                }
                return true;
            }, (column, prop) =>
             {
                 column.DbColumnName = prop.Name;
                 column.PropertyName = prop.Name;
                 column.PropertyType = prop.PropertyType;
                 SugarColumn sugarColumn = prop.GetAttributeInfo<SugarColumn>();
                 if (sugarColumn != null)
                 {
                     column.DbColumnName = sugarColumn.ColumnName;
                 }
             });
        }

        /// <summary>
        /// 表名
        /// </summary>
        public string TableName
        {
            get
            {
                return _tableName;
            }
        }
        /// <summary>
        /// Ado
        /// </summary>
        public IAdo Ado { get; set; }

        /// <summary>
        /// 以Insert into select的形式新增
        /// </summary>
        public InsertBy InsertBy { get; set; } = InsertBy.ProvidedValues;

        /// <summary>
        /// 当IsBySelect=true时，需要用到这个
        /// </summary>
        public QueryBuilder QueryBuilder
        {
            get { return _queryBuilder; }
            set
            {
                if (value != null)
                {
                    if (value.SelectValue is LambdaExpression lambdaExpression)
                    {
                        switch (lambdaExpression.Body.NodeType)
                        {
                            case ExpressionType.New:
                                NewExpression newExpression = lambdaExpression.Body as NewExpression;
                                if (newExpression.Members.Count > 0)
                                {
                                    _dbColumnInfos.Clear();
                                    _dbColumnInfos.ConcatWith(newExpression.Members, member =>
                                    {
                                        if (member is PropertyInfo property)
                                        {
                                            SugarColumn sugarColumn = property.GetAttributeInfo<SugarColumn>();
                                            if (sugarColumn != null)
                                            {
                                                return !sugarColumn.IsOnlyIgnoreInsert;
                                            }
                                            return true;
                                        }
                                        return false;
                                    }, (column, member) =>
                                    {
                                        PropertyInfo property = member as PropertyInfo;
                                        column.DbColumnName = property.Name;
                                        column.PropertyName = property.Name;
                                        column.PropertyType = property.PropertyType;
                                        SugarColumn sugarColumn = property.GetAttributeInfo<SugarColumn>();
                                        if (sugarColumn != null)
                                        {
                                            column.DbColumnName = sugarColumn.ColumnName;
                                        }
                                    });
                                }
                                break;
                            case ExpressionType.Parameter:
                                ParameterExpression parameterExpression = lambdaExpression.Body as ParameterExpression;
                                _dbColumnInfos.Clear();
                                _dbColumnInfos.ConcatWith(parameterExpression.Type.GetProperties(), prop =>
                                {
                                    SugarColumn sugarColumn = prop.GetAttributeInfo<SugarColumn>();
                                    if (sugarColumn != null)
                                    {
                                        return !sugarColumn.IsOnlyIgnoreInsert;
                                    }
                                    return true;
                                }, (column, prop) =>
                                {
                                    column.DbColumnName = prop.Name;
                                    column.PropertyName = prop.Name;
                                    column.PropertyType = prop.PropertyType;
                                    SugarColumn sugarColumn = prop.GetAttributeInfo<SugarColumn>();
                                    if (sugarColumn != null)
                                    {
                                        column.DbColumnName = sugarColumn.ColumnName;
                                    }
                                });
                                break;
                        }
                    }
                }
                _queryBuilder = value;
            }
        }

        /// <summary>
        /// 需要新增的列
        /// </summary>
        public List<DbColumnInfo> Columns
        {
            get
            {
                if (IgnoreColumns.Any())
                {
                    _dbColumnInfos.RemoveAll(x => IgnoreColumns.Any(y => x.DbColumnName == y.DbColumnName
                                            && x.PropertyName == y.PropertyName));
                }
                return _dbColumnInfos;
                ;
            }
            set { _dbColumnInfos = value; }
        }

        public List<DbColumnInfo> IgnoreColumns { get; set; } = new List<DbColumnInfo>();

        /// <summary>
        /// 需要新增的值
        /// </summary>
        public List<T> Values { get; set; }


    }
}
