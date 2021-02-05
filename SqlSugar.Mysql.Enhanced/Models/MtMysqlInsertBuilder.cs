using SqlSugar;
using Sqsugar.Mysql.Enhanced.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sqsugar.Mysql.Enhanced.Models
{
    public class MtMysqlInsertBuilder<T>
    {
        private string table;
        public string Table
        {
            get { return table; }
            set { table = value; }
        }
        public string PrefixParams { get; set; } = "@";
        public List<DbColumnInfo> Columns { get; set; }
        public List<T> Values { get; set; }
        public string Left { get; set; } = "`";
        public string Right { get; set; } = "`";
        public ModifyType InsertType { get; set; } = ModifyType.DirectInsert;
        public InsertedResultType InsertedResultType { get; set; } = InsertedResultType.AffectedRowsCount;
        public InsertBy InsertBy { get; set; } = InsertBy.ProvidedValues;
        public QueryBuilder QueryBuilder { get; set; }
        /// <summary>
        /// 获取表名
        /// </summary>
        /// <returns></returns>
        public string GetTableSql()
        {
            StringBuilder sb = new StringBuilder();
            string tableName = table;
            if (string.IsNullOrWhiteSpace(tableName))
            {
                Type type = typeof(T);
                if (Attribute.GetCustomAttributes(type).Any(x => x is SugarTable))
                {
                    SugarTable table = Attribute.GetCustomAttributes(type).FirstOrDefault(x => x is SugarTable) as SugarTable;
                    if (!string.IsNullOrWhiteSpace(table.TableName))
                    {
                        tableName = table.TableName;
                    }
                }
            }
            if (tableName.IndexOf(Left) == -1)
            {
                sb.Append(Left);
            }
            sb.Append(tableName);
            if (tableName.IndexOf(Right) == -1)
            {
                sb.Append(Right);
            }
            return sb.ToString();
        }
        /// <summary>
        /// 获取新增列
        /// </summary>
        /// <returns></returns>
        public string GetColumnsSql()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(" + string.Join(",", Columns.Select(x => $"{Left}{x.DbColumnName}{Right}")) + ")");
            return sb.ToString();
        }
        /// <summary>
        /// 获取每行数据的参数
        /// </summary>
        /// <returns></returns>
        private List<List<SugarParameter>> GetParametersByRow()
        {
            List<List<SugarParameter>> parametersList = new List<List<SugarParameter>>();
            switch (InsertBy)
            {
                case InsertBy.SelectQuery:
                    parametersList.Add(QueryBuilder.Parameters);
                    break;
                case InsertBy.ProvidedValues:
                    Type type = typeof(T);
                    List<PropertyInfo> props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
                    for (int i = 0; i < Values.Count; i++)
                    {
                        List<SugarParameter> parameters = new List<SugarParameter>();
                        foreach (DbColumnInfo colInfo in Columns)
                        {
                            SugarParameter sugarParameter = new SugarParameter(PrefixParams + colInfo.DbColumnName + i, null);
                            PropertyInfo prop = props.FirstOrDefault(x =>
                            {
                                string colName = x.Name;
                                if (x.GetCustomAttributes().Any(x => x is SugarColumn))
                                {
                                    SugarColumn column = x.GetCustomAttributes().First(x => x is SugarColumn) as SugarColumn;
                                    if (!string.IsNullOrWhiteSpace(column.ColumnName))
                                    {
                                        colName = column.ColumnName;
                                    }
                                }
                                return colName.Equals(colInfo.DbColumnName, StringComparison.OrdinalIgnoreCase);
                            });
                            if (prop != null)
                            {
                                sugarParameter.Value = prop.GetValue(Values[i]);
                            }
                            parameters.Add(sugarParameter);
                        }
                        parametersList.Add(parameters);
                    }
                    break;
            }
            return parametersList;

        }
        /// <summary>
        /// 获取新增数据的值
        /// </summary>
        /// <returns></returns>
        public string GetInsertValuesSql()
        {
            StringBuilder sb = new StringBuilder();
            switch (InsertBy)
            {
                case InsertBy.SelectQuery:
                    sb.Append(QueryBuilder.ToSqlString());
                    break;
                case InsertBy.ProvidedValues:
                    var paras = GetParametersByRow();
                    sb.Append("VALUES");
                    sb.Append(string.Join(",", paras.Select(x => "(" + string.Join(",", x.Select(y => y.ParameterName)) + ")")));
                    break;
            }
            return sb.ToString();
        }
        public string GetUpdateValuesSql()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Join(",", Columns.Select(x => $"{Left}{x.DbColumnName}{Right}=VALUES({Left}{x.DbColumnName}{Right})")));
            return sb.ToString();
        }
        public string BuildSql()
        {
            StringBuilder sb = new StringBuilder();
            switch (InsertType)
            {
                case ModifyType.UpdateExisted:
                    sb.Append($"INSERT INTO {GetTableSql()}{GetColumnsSql()} {GetInsertValuesSql()} ON DUPLICATE KEY UPDATE {GetUpdateValuesSql()};");
                    break;
                case ModifyType.IgnoreExisted:
                    sb.Append($"INSERT IGNORE INTO {GetTableSql()}{GetColumnsSql()} {GetInsertValuesSql()};");
                    break;
                case ModifyType.Replace:
                    sb.Append($"REPLACE INTO {GetTableSql()}{GetColumnsSql()} {GetInsertValuesSql()};");
                    break;
                case ModifyType.DirectInsert:
                default:
                    sb.Append($"INSERT INTO {GetTableSql()}{GetColumnsSql()} {GetInsertValuesSql()};");
                    break;
            }
            switch (InsertedResultType)
            {
                case InsertedResultType.AffectedLastId:
                    sb.Append("SELECT LAST_INSERT_ID();");
                    break;
                case InsertedResultType.AffectedAllId:
                    if (this.InsertBy == InsertBy.ProvidedValues)
                    {
                        int? count = Values?.Count;
                        string primaryKey = string.Empty;
                        PropertyInfo[] properties = typeof(T).GetProperties();
                        PropertyInfo sugarProperty = properties.FirstOrDefault(p => p.GetCustomAttributes().Any(a => a is SqlSugar.SugarColumn sc && sc.IsPrimaryKey));
                        primaryKey = sugarProperty.Name;
                        if (sugarProperty != null)
                        {
                            SugarColumn sugarColumn = sugarProperty.GetCustomAttributes().First(a => a is SqlSugar.SugarColumn sc && sc.IsPrimaryKey) as SugarColumn;
                            if (!string.IsNullOrWhiteSpace(sugarColumn.ColumnName))
                            {
                                primaryKey = sugarColumn.ColumnName;
                            }
                        }
                        if (primaryKey == string.Empty || count == null || count == 0)
                        {
                            sb.Append("SELECT -1 as Id;");
                        }
                        else
                        {
                            sb.Append($"SELECT `{primaryKey}` AS Id FROM {GetTableSql()} ORDER BY {primaryKey} DESC LIMIT {count};");
                        }
                    }
                    break;
                case InsertedResultType.AffectedAllEntity:
                    if (this.InsertBy == InsertBy.ProvidedValues)
                    {
                        int? count = Values?.Count;
                        string primaryKey = string.Empty;
                        PropertyInfo[] properties = typeof(T).GetProperties();
                        PropertyInfo sugarProperty = properties.FirstOrDefault(p => p.GetCustomAttributes().Any(a => a is SqlSugar.SugarColumn sc && sc.IsPrimaryKey));
                        primaryKey = sugarProperty.Name;
                        if (sugarProperty != null)
                        {
                            SugarColumn sugarColumn = sugarProperty.GetCustomAttributes().First(a => a is SqlSugar.SugarColumn sc && sc.IsPrimaryKey) as SugarColumn;
                            if (!string.IsNullOrWhiteSpace(sugarColumn.ColumnName))
                            {
                                primaryKey = sugarColumn.ColumnName;
                            }
                        }
                        if (primaryKey == string.Empty || count == null || count == 0)
                        {
                            sb.Append("SELECT -1 as Id;");
                        }
                        else
                        {
                            sb.Append($"SELECT * FROM {GetTableSql()} ORDER BY {primaryKey} DESC LIMIT {count};");
                        }
                        sb.Append($"SELECT * FROM {GetTableSql()} ORDER BY {primaryKey} DESC LIMIT {count};");
                    }
                    break;
            }
            return sb.ToString();
        }
        public IEnumerable<SugarParameter> GetParameters()
        {
            foreach (var rowParam in GetParametersByRow())
            {
                foreach (var colParam in rowParam)
                {
                    yield return colParam;
                }
            }
        }
    }
}
