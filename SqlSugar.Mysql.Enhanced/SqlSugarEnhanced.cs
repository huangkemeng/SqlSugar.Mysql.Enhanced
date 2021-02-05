using Microsoft.AspNetCore.Http;
using SqlSugar;
using Sqsugar.Mysql.Enhanced.Enums;
using Sqsugar.Mysql.Enhanced.Helpers;
using Sqsugar.Mysql.Enhanced.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Sqsugar.Mysql.Enhanced
{
    public static class SqlSugarEnhanced
    {
        private readonly static IHttpContextAccessor httpAccessor;
        static SqlSugarEnhanced()
        {
            httpAccessor = ServiceAccessor.Get<IHttpContextAccessor>();
        }

        #region SmartQueryAsync
        public async static Task<List<T>> ToSmartListAsync<T>(this ISugarQueryable<T> sugarQueryable, RefAsync<int> total)
        {
            try
            {
                //当前页
                string pIndex = httpAccessor.HttpContext.Request.Query["pageIndex"];
                //每页的行数
                string pSize = httpAccessor.HttpContext.Request.Query["pageSize"];
                int pageIndex = -1;
                int pageSize = -1;
                ///排序的字段
                string orderBy = httpAccessor.HttpContext.Request.Query["orderBy"];
                ///排序的方向 asc 或 desc
                string orderDir = httpAccessor.HttpContext.Request.Query["orderDir"];
                if (!string.IsNullOrWhiteSpace(orderDir) && "DESC".Equals(orderDir, StringComparison.OrdinalIgnoreCase))
                {
                    orderDir = "DESC";
                }
                else
                {
                    orderDir = "ASC";
                }
                if (!string.IsNullOrWhiteSpace(orderBy))
                {
                    var selectStrings = sugarQueryable.QueryBuilder.GetSelectValue.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                    string left = sugarQueryable.QueryBuilder.Builder.SqlTranslationLeft;
                    string right = sugarQueryable.QueryBuilder.Builder.SqlTranslationRight;
                    if (selectStrings.Any(x => x.Contains($"{left}{orderBy}{right}", StringComparison.OrdinalIgnoreCase)))
                    {
                        sugarQueryable.QueryBuilder.OrderByValue = $"{sugarQueryable.QueryBuilder.OrderByTemplate} {orderBy.ToUpper()} {orderDir}";
                    }
                }
                if (!string.IsNullOrWhiteSpace(pIndex) && !string.IsNullOrWhiteSpace(pSize))
                {
                    int.TryParse(pIndex, out pageIndex);
                    int.TryParse(pSize, out pageSize);
                }
                if (pageIndex != -1 && pageSize != -1)
                {
                    return await sugarQueryable.ToPageListAsync(pageIndex, pageSize, total);
                }
                else
                {
                    var result = await sugarQueryable.ToListAsync();
                    total.Value = result.Count;
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async static Task<string> ToSmartJsonAsync<T>(this ISugarQueryable<T> sugarQueryable, RefAsync<int> total)
        {
            string pIndex = httpAccessor.HttpContext.Request.Query["pageIndex"];
            string pSize = httpAccessor.HttpContext.Request.Query["pageSize"];
            int pageIndex = -1;
            int pageSize = -1;
            string orderBy = httpAccessor.HttpContext.Request.Query["orderBy"];
            string orderDir = httpAccessor.HttpContext.Request.Query["orderDir"];
            if (!string.IsNullOrWhiteSpace(orderDir) && "DESC".Equals(orderDir, StringComparison.OrdinalIgnoreCase))
            {
                orderDir = "DESC";
            }
            else
            {
                orderDir = "ASC";
            }
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                var selectStrings = sugarQueryable.QueryBuilder.GetSelectValue.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                string left = sugarQueryable.QueryBuilder.Builder.SqlTranslationLeft;
                string right = sugarQueryable.QueryBuilder.Builder.SqlTranslationRight;
                if (selectStrings.Any(x => x.Contains($"{left}{orderBy}{right}", StringComparison.OrdinalIgnoreCase)))
                {
                    sugarQueryable.QueryBuilder.OrderByValue = $"{sugarQueryable.QueryBuilder.OrderByTemplate} {orderBy.ToUpper()} {orderDir}";
                }
            }
            if (!string.IsNullOrWhiteSpace(pIndex) && !string.IsNullOrWhiteSpace(pSize))
            {
                int.TryParse(pIndex, out pageIndex);
                int.TryParse(pSize, out pageSize);
            }
            if (pageIndex != -1 && pageSize != -1)
            {
                return await sugarQueryable.ToJsonPageAsync(pageIndex, pageSize, total);
            }
            else
            {
                var result = await sugarQueryable.ToListAsync();
                total.Value = result.Count;
                return result.ToJson();
            }
        }
        public async static Task<DataTable> ToSmartDataTableAsync<T>(this ISugarQueryable<T> sugarQueryable, RefAsync<int> total)
        {
            string pIndex = httpAccessor.HttpContext.Request.Query["pageIndex"];
            string pSize = httpAccessor.HttpContext.Request.Query["pageSize"];
            int pageIndex = -1;
            int pageSize = -1;
            string orderBy = httpAccessor.HttpContext.Request.Query["orderBy"];
            string orderDir = httpAccessor.HttpContext.Request.Query["orderDir"];
            if (!string.IsNullOrWhiteSpace(orderDir) && "DESC".Equals(orderDir, StringComparison.OrdinalIgnoreCase))
            {
                orderDir = "DESC";
            }
            else
            {
                orderDir = "ASC";
            }
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                var selectStrings = sugarQueryable.QueryBuilder.GetSelectValue.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                string left = sugarQueryable.QueryBuilder.Builder.SqlTranslationLeft;
                string right = sugarQueryable.QueryBuilder.Builder.SqlTranslationRight;
                if (selectStrings.Any(x => x.Contains($"{left}{orderBy}{right}", StringComparison.OrdinalIgnoreCase)))
                {
                    sugarQueryable.QueryBuilder.OrderByValue = $"{sugarQueryable.QueryBuilder.OrderByTemplate} {orderBy.ToUpper()} {orderDir}";
                }
            }
            if (!string.IsNullOrWhiteSpace(pIndex) && !string.IsNullOrWhiteSpace(pSize))
            {
                int.TryParse(pIndex, out pageIndex);
                int.TryParse(pSize, out pageSize);
            }
            if (pageIndex != -1 && pageSize != -1)
            {
                return await sugarQueryable.ToDataTablePageAsync(pageIndex, pageSize, total);
            }
            else
            {
                var result = await sugarQueryable.ToDataTableAsync();
                total.Value = result.Rows.Count;
                return result;
            }
        }
        #endregion

        #region SmartQuery
        public static List<T> ToSmartList<T>(this ISugarQueryable<T> sugarQueryable, out int total)
        {
            string pIndex = httpAccessor.HttpContext.Request.Query["pageIndex"];
            string pSize = httpAccessor.HttpContext.Request.Query["pageSize"];
            int pageIndex = -1;
            int pageSize = -1;
            int reftotal = 0;
            string orderBy = httpAccessor.HttpContext.Request.Query["orderBy"];
            string orderDir = httpAccessor.HttpContext.Request.Query["orderDir"];
            if (!string.IsNullOrWhiteSpace(orderDir) && "DESC".Equals(orderDir, StringComparison.OrdinalIgnoreCase))
            {
                orderDir = "DESC";
            }
            else
            {
                orderDir = "ASC";
            }
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                var selectStrings = sugarQueryable.QueryBuilder.GetSelectValue.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                string left = sugarQueryable.QueryBuilder.Builder.SqlTranslationLeft;
                string right = sugarQueryable.QueryBuilder.Builder.SqlTranslationRight;
                if (selectStrings.Any(x => x.Contains($"{left}{orderBy}{right}", StringComparison.OrdinalIgnoreCase)))
                {
                    sugarQueryable.QueryBuilder.OrderByValue = $"{sugarQueryable.QueryBuilder.OrderByTemplate} {orderBy.ToUpper()} {orderDir}";
                }
            }
            if (!string.IsNullOrWhiteSpace(pIndex) && !string.IsNullOrWhiteSpace(pSize))
            {
                int.TryParse(pIndex, out pageIndex);
                int.TryParse(pSize, out pageSize);
            }
            if (pageIndex != -1 && pageSize != -1)
            {
                var result = sugarQueryable.ToPageList(pageIndex, pageSize, ref reftotal);
                total = reftotal;
                return result;
            }
            else
            {
                var result = sugarQueryable.ToList();
                total = result.Count;
                return result;
            }
        }
        public static string ToSmartJson<T>(this ISugarQueryable<T> sugarQueryable, out int total)
        {
            string pIndex = httpAccessor.HttpContext.Request.Query["pageIndex"];
            string pSize = httpAccessor.HttpContext.Request.Query["pageSize"];
            int pageIndex = -1;
            int pageSize = -1;
            int reftotal = 0;
            string orderBy = httpAccessor.HttpContext.Request.Query["orderBy"];
            string orderDir = httpAccessor.HttpContext.Request.Query["orderDir"];
            if (!string.IsNullOrWhiteSpace(orderDir) && "DESC".Equals(orderDir, StringComparison.OrdinalIgnoreCase))
            {
                orderDir = "DESC";
            }
            else
            {
                orderDir = "ASC";
            }
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                var selectStrings = sugarQueryable.QueryBuilder.GetSelectValue.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                string left = sugarQueryable.QueryBuilder.Builder.SqlTranslationLeft;
                string right = sugarQueryable.QueryBuilder.Builder.SqlTranslationRight;
                if (selectStrings.Any(x => x.Contains($"{left}{orderBy}{right}", StringComparison.OrdinalIgnoreCase)))
                {
                    sugarQueryable.QueryBuilder.OrderByValue = $"{sugarQueryable.QueryBuilder.OrderByTemplate} {orderBy.ToUpper()} {orderDir}";
                }
            }
            if (!string.IsNullOrWhiteSpace(pIndex) && !string.IsNullOrWhiteSpace(pSize))
            {
                int.TryParse(pIndex, out pageIndex);
                int.TryParse(pSize, out pageSize);
            }
            if (pageIndex != -1 && pageSize != -1)
            {
                var result = sugarQueryable.ToJsonPage(pageIndex, pageSize, ref reftotal);
                total = reftotal;
                return result;
            }
            else
            {
                var result = sugarQueryable.ToList();
                total = result.Count;
                return result.ToJson();
            }
        }
        public static DataTable ToSmartDataTable<T>(this ISugarQueryable<T> sugarQueryable, out int total)
        {
            string pIndex = httpAccessor.HttpContext.Request.Query["pageIndex"];
            string pSize = httpAccessor.HttpContext.Request.Query["pageSize"];
            int pageIndex = -1;
            int pageSize = -1;
            int reftotal = 0;
            string orderBy = httpAccessor.HttpContext.Request.Query["orderBy"];
            string orderDir = httpAccessor.HttpContext.Request.Query["orderDir"];
            if (!string.IsNullOrWhiteSpace(orderDir) && "DESC".Equals(orderDir, StringComparison.OrdinalIgnoreCase))
            {
                orderDir = "DESC";
            }
            else
            {
                orderDir = "ASC";
            }
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                var selectStrings = sugarQueryable.QueryBuilder.GetSelectValue.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                string left = sugarQueryable.QueryBuilder.Builder.SqlTranslationLeft;
                string right = sugarQueryable.QueryBuilder.Builder.SqlTranslationRight;
                if (selectStrings.Any(x => x.Contains($"{left}{orderBy}{right}", StringComparison.OrdinalIgnoreCase)))
                {
                    sugarQueryable.QueryBuilder.OrderByValue = $"{sugarQueryable.QueryBuilder.OrderByTemplate} {orderBy.ToUpper()} {orderDir}";
                }
            }
            if (!string.IsNullOrWhiteSpace(pIndex) && !string.IsNullOrWhiteSpace(pSize))
            {
                int.TryParse(pIndex, out pageIndex);
                int.TryParse(pSize, out pageSize);
            }
            if (pageIndex != -1 && pageSize != -1)
            {
                var result = sugarQueryable.ToDataTablePage(pageIndex, pageSize, ref reftotal);
                total = reftotal;
                return result;
            }
            else
            {
                var result = sugarQueryable.ToDataTable();
                total = result.Rows.Count;
                return result;
            }
        }
        #endregion

        #region SmartWhere
        public static ISugarQueryable<T> SmartWhere<T>(this ISugarQueryable<T> queryable, Expression<Func<T, bool>> expression)
        {
            var bodyExpr = expression.Body;
            int count = queryable.QueryBuilder.Parameters.Count;
            SqlResult sqlResult = SqlExtension.ExpressionToWhereSql(bodyExpr, ref count, "");
            queryable.AddParameters(sqlResult.Params);
            string where;
            if (queryable.QueryBuilder.WhereInfos.Any())
            {
                where = " AND ";
            }
            else
            {
                where = " WHERE ";
            }
            queryable.QueryBuilder.WhereInfos.Add($"{where}{sqlResult.SqlString}");
            return queryable;
        }
        public static ISugarQueryable<T> SmartWhere<T, TParam>(this ISugarQueryable<T> queryable, Expression<Func<T, TParam>> expression, params TParam[] param)
        {
            var paras = expression.Parameters;
            var body = expression.Body;
            if (param != null && param.Length > 0)
            {
                if (body is MemberExpression memberExpression)
                {
                    if (param.Length == 1)
                    {
                        ConstantExpression constantExpression = Expression.Constant(param[0], typeof(TParam));
                        BinaryExpression eqExpression = Expression.Equal(memberExpression, constantExpression);
                        Expression<Func<T, bool>> bool_expression = Expression.Lambda<Func<T, bool>>(eqExpression, paras);
                        queryable.Where(bool_expression);
                    }
                    else
                    {
                        UnaryExpression unaryExpression = Expression.Convert(memberExpression, typeof(object));
                        Expression<Func<T, object>> mExpression = Expression.Lambda<Func<T, object>>(unaryExpression, paras);
                        queryable.In(mExpression, param);
                    }
                }
            }
            return queryable;
        }
        public static ISugarQueryable<T, T1> SmartWhere<T, T1, TParam>(this ISugarQueryable<T, T1> queryable, Expression<Func<T, T1, TParam>> expression, params TParam[] param)
        {
            var paras = expression.Parameters;
            var body = expression.Body;
            if (param != null && param.Length > 0)
            {
                if (body is MemberExpression memberExpression)
                {
                    if (param.Length == 1)
                    {
                        ConstantExpression constantExpression = Expression.Constant(param[0], typeof(TParam));
                        BinaryExpression eqExpression = Expression.Equal(memberExpression, constantExpression);
                        Expression<Func<T, T1, bool>> bool_expression = Expression.Lambda<Func<T, T1, bool>>(eqExpression, paras);
                        queryable.Where(bool_expression);
                    }
                    else
                    {
                        UnaryExpression unaryExpression = Expression.Convert(memberExpression, typeof(object));
                        Expression<Func<T, T1, object>> mExpression = Expression.Lambda<Func<T, T1, object>>(unaryExpression, paras);
                        queryable.In(mExpression, param);
                    }
                }
            }
            return queryable;
        }
        public static ISugarQueryable<T, T1, T2> SmartWhere<T, T1, T2, TParam>(this ISugarQueryable<T, T1, T2> queryable, Expression<Func<T, T1, T2, TParam>> expression, params TParam[] param)
        {
            var paras = expression.Parameters;
            var body = expression.Body;
            if (param != null && param.Length > 0)
            {
                if (body is MemberExpression memberExpression)
                {
                    if (param.Length == 1)
                    {
                        ConstantExpression constantExpression = Expression.Constant(param[0], typeof(TParam));
                        BinaryExpression eqExpression = Expression.Equal(memberExpression, constantExpression);
                        Expression<Func<T, T1, T2, bool>> bool_expression = Expression.Lambda<Func<T, T1, T2, bool>>(eqExpression, paras);
                        queryable.Where(bool_expression);
                    }
                    else
                    {
                        UnaryExpression unaryExpression = Expression.Convert(memberExpression, typeof(object));
                        Expression<Func<T, T1, T2, object>> mExpression = Expression.Lambda<Func<T, T1, T2, object>>(unaryExpression, paras);
                        queryable.In(mExpression, param);
                    }
                }
            }
            return queryable;
        }
        public static ISugarQueryable<T, T1, T2, T3> SmartWhere<T, T1, T2, T3, TParam>(this ISugarQueryable<T, T1, T2, T3> queryable, Expression<Func<T, T1, T2, T3, TParam>> expression, params TParam[] param)
        {
            var paras = expression.Parameters;
            var body = expression.Body;
            if (param != null && param.Length > 0)
            {
                if (body is MemberExpression memberExpression)
                {
                    if (param.Length == 1)
                    {
                        ConstantExpression constantExpression = Expression.Constant(param[0], typeof(TParam));
                        BinaryExpression eqExpression = Expression.Equal(memberExpression, constantExpression);
                        Expression<Func<T, T1, T2, T3, bool>> bool_expression = Expression.Lambda<Func<T, T1, T2, T3, bool>>(eqExpression, paras);
                        queryable.Where(bool_expression);
                    }
                    else
                    {
                        UnaryExpression unaryExpression = Expression.Convert(memberExpression, typeof(object));
                        Expression<Func<T, T1, T2, T3, object>> mExpression = Expression.Lambda<Func<T, T1, T2, T3, object>>(unaryExpression, paras);
                        queryable.In(mExpression, param);
                    }
                }
            }
            return queryable;
        }
        public static ISugarQueryable<T, T1, T2, T3, T4> SmartWhere<T, T1, T2, T3, T4, TParam>(this ISugarQueryable<T, T1, T2, T3, T4> queryable, Expression<Func<T, T1, T2, T3, T4, TParam>> expression, params TParam[] param)
        {
            var paras = expression.Parameters;
            var body = expression.Body;
            if (param != null && param.Length > 0)
            {
                if (body is MemberExpression memberExpression)
                {
                    ConstantExpression constantExpression = Expression.Constant(param[0], typeof(TParam));
                    BinaryExpression eqExpression = Expression.Equal(memberExpression, constantExpression);
                    for (int i = 1; i < param.Length; i++)
                    {
                        ConstantExpression orConstantExpression = Expression.Constant(param[i], typeof(TParam));
                        BinaryExpression orEqExpression = Expression.Equal(memberExpression, orConstantExpression);
                        eqExpression = Expression.Or(eqExpression, orEqExpression);
                    }
                    Expression<Func<T, T1, T2, T3, T4, bool>> boolExpression = Expression.Lambda<Func<T, T1, T2, T3, T4, bool>>(eqExpression, paras);
                    queryable.Where(boolExpression);
                }
            }
            return queryable;
        }
        public static ISugarQueryable<T, T1, T2, T3, T4, T5> SmartWhere<T, T1, T2, T3, T4, T5, TParam>(this ISugarQueryable<T, T1, T2, T3, T4, T5> queryable, Expression<Func<T, T1, T2, T3, T4, T5, TParam>> expression, params TParam[] param)
        {
            var paras = expression.Parameters;
            var body = expression.Body;
            if (param != null && param.Length > 0)
            {
                if (body is MemberExpression memberExpression)
                {
                    ConstantExpression constantExpression = Expression.Constant(param[0], typeof(TParam));
                    BinaryExpression eqExpression = Expression.Equal(memberExpression, constantExpression);
                    for (int i = 1; i < param.Length; i++)
                    {
                        ConstantExpression orConstantExpression = Expression.Constant(param[i], typeof(TParam));
                        BinaryExpression orEqExpression = Expression.Equal(memberExpression, orConstantExpression);
                        eqExpression = Expression.Or(eqExpression, orEqExpression);
                    }
                    Expression<Func<T, T1, T2, T3, T4, T5, bool>> boolExpression = Expression.Lambda<Func<T, T1, T2, T3, T4, T5, bool>>(eqExpression, paras);
                    queryable.Where(boolExpression);
                }
            }
            return queryable;
        }
        public static ISugarQueryable<T, T1, T2, T3, T4, T5, T6> SmartWhere<T, T1, T2, T3, T4, T5, T6, TParam>(this ISugarQueryable<T, T1, T2, T3, T4, T5, T6> queryable, Expression<Func<T, T1, T2, T3, T4, T5, T6, TParam>> expression, params TParam[] param)
        {
            var paras = expression.Parameters;
            var body = expression.Body;
            if (param != null && param.Length > 0)
            {
                if (body is MemberExpression memberExpression)
                {
                    ConstantExpression constantExpression = Expression.Constant(param[0], typeof(TParam));
                    BinaryExpression eqExpression = Expression.Equal(memberExpression, constantExpression);
                    for (int i = 1; i < param.Length; i++)
                    {
                        ConstantExpression orConstantExpression = Expression.Constant(param[i], typeof(TParam));
                        BinaryExpression orEqExpression = Expression.Equal(memberExpression, orConstantExpression);
                        eqExpression = Expression.Or(eqExpression, orEqExpression);
                    }
                    Expression<Func<T, T1, T2, T3, T4, T5, T6, bool>> boolExpression = Expression.Lambda<Func<T, T1, T2, T3, T4, T5, T6, bool>>(eqExpression, paras);
                    queryable.Where(boolExpression);
                }
            }
            return queryable;
        }
        public static ISugarQueryable<T, T1, T2, T3, T4, T5, T6, T7> SmartWhere<T, T1, T2, T3, T4, T5, T6, T7, TParam>(this ISugarQueryable<T, T1, T2, T3, T4, T5, T6, T7> queryable, Expression<Func<T, T1, T2, T3, T4, T5, T6, T7, TParam>> expression, params TParam[] param)
        {
            var paras = expression.Parameters;
            var body = expression.Body;
            if (param != null && param.Length > 0)
            {
                if (body is MemberExpression memberExpression)
                {
                    ConstantExpression constantExpression = Expression.Constant(param[0], typeof(TParam));
                    BinaryExpression eqExpression = Expression.Equal(memberExpression, constantExpression);
                    for (int i = 1; i < param.Length; i++)
                    {
                        ConstantExpression orConstantExpression = Expression.Constant(param[i], typeof(TParam));
                        BinaryExpression orEqExpression = Expression.Equal(memberExpression, orConstantExpression);
                        eqExpression = Expression.Or(eqExpression, orEqExpression);
                    }
                    Expression<Func<T, T1, T2, T3, T4, T5, T6, T7, bool>> boolExpression = Expression.Lambda<Func<T, T1, T2, T3, T4, T5, T6, T7, bool>>(eqExpression, paras);
                    queryable.Where(boolExpression);
                }
            }
            return queryable;
        }
        public static ISugarQueryable<T, T1, T2, T3, T4, T5, T6, T7, T8> SmartWhere<T, T1, T2, T3, T4, T5, T6, T7, T8, TParam>(this ISugarQueryable<T, T1, T2, T3, T4, T5, T6, T7, T8> queryable, Expression<Func<T, T1, T2, T3, T4, T5, T6, T7, T8, TParam>> expression, params TParam[] param)
        {
            var paras = expression.Parameters;
            var body = expression.Body;
            if (param != null && param.Length > 0)
            {
                if (body is MemberExpression memberExpression)
                {
                    ConstantExpression constantExpression = Expression.Constant(param[0], typeof(TParam));
                    BinaryExpression eqExpression = Expression.Equal(memberExpression, constantExpression);
                    for (int i = 1; i < param.Length; i++)
                    {
                        ConstantExpression orConstantExpression = Expression.Constant(param[i], typeof(TParam));
                        BinaryExpression orEqExpression = Expression.Equal(memberExpression, orConstantExpression);
                        eqExpression = Expression.Or(eqExpression, orEqExpression);
                    }
                    Expression<Func<T, T1, T2, T3, T4, T5, T6, T7, T8, bool>> boolExpression = Expression.Lambda<Func<T, T1, T2, T3, T4, T5, T6, T7, T8, bool>>(eqExpression, paras);
                    queryable.Where(boolExpression);
                }
            }
            return queryable;
        }
        public static ISugarQueryable<T, T1, T2, T3, T4, T5, T6, T7, T8, T9> SmartWhere<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, TParam>(this ISugarQueryable<T, T1, T2, T3, T4, T5, T6, T7, T8, T9> queryable, Expression<Func<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, TParam>> expression, params TParam[] param)
        {
            var paras = expression.Parameters;
            var body = expression.Body;
            if (param != null && param.Length > 0)
            {
                if (body is MemberExpression memberExpression)
                {
                    ConstantExpression constantExpression = Expression.Constant(param[0], typeof(TParam));
                    BinaryExpression eqExpression = Expression.Equal(memberExpression, constantExpression);
                    for (int i = 1; i < param.Length; i++)
                    {
                        ConstantExpression orConstantExpression = Expression.Constant(param[i], typeof(TParam));
                        BinaryExpression orEqExpression = Expression.Equal(memberExpression, orConstantExpression);
                        eqExpression = Expression.Or(eqExpression, orEqExpression);
                    }
                    Expression<Func<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> boolExpression = Expression.Lambda<Func<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>(eqExpression, paras);
                    queryable.Where(boolExpression);
                }
            }
            return queryable;
        }
        public static ISugarQueryable<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> SmartWhere<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TParam>(this ISugarQueryable<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> queryable, Expression<Func<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TParam>> expression, params TParam[] param)
        {
            var paras = expression.Parameters;
            var body = expression.Body;
            if (param != null && param.Length > 0)
            {
                if (body is MemberExpression memberExpression)
                {
                    ConstantExpression constantExpression = Expression.Constant(param[0], typeof(TParam));
                    BinaryExpression eqExpression = Expression.Equal(memberExpression, constantExpression);
                    for (int i = 1; i < param.Length; i++)
                    {
                        ConstantExpression orConstantExpression = Expression.Constant(param[i], typeof(TParam));
                        BinaryExpression orEqExpression = Expression.Equal(memberExpression, orConstantExpression);
                        eqExpression = Expression.Or(eqExpression, orEqExpression);
                    }
                    Expression<Func<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> boolExpression = Expression.Lambda<Func<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>(eqExpression, paras);
                    queryable.Where(boolExpression);
                }
            }
            return queryable;
        }
        public static ISugarQueryable<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> SmartWhere<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TParam>(this ISugarQueryable<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> queryable, Expression<Func<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TParam>> expression, params TParam[] param)
        {
            var paras = expression.Parameters;
            var body = expression.Body;
            if (param != null && param.Length > 0)
            {
                if (body is MemberExpression memberExpression)
                {
                    ConstantExpression constantExpression = Expression.Constant(param[0], typeof(TParam));
                    BinaryExpression eqExpression = Expression.Equal(memberExpression, constantExpression);
                    for (int i = 1; i < param.Length; i++)
                    {
                        ConstantExpression orConstantExpression = Expression.Constant(param[i], typeof(TParam));
                        BinaryExpression orEqExpression = Expression.Equal(memberExpression, orConstantExpression);
                        eqExpression = Expression.Or(eqExpression, orEqExpression);
                    }
                    Expression<Func<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> boolExpression = Expression.Lambda<Func<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>(eqExpression, paras);
                    queryable.Where(boolExpression);
                }
            }
            return queryable;
        }
        #endregion

        #region IgnoreColumns
        public static IInsertable<T> IgnoreColumns<T, T1>(this IInsertable<T> insertable, T1 obj, bool where = true)
        {
            if (where)
            {
                Type t1Type = typeof(T1);
                PropertyInfo[] props = t1Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (props.Length > 0)
                {
                    foreach (var item in props)
                    {
                        if (!insertable.InsertBuilder.Context.IgnoreInsertColumns.Any(x => x.PropertyName == item.Name))
                        {
                            insertable.InsertBuilder.Context.IgnoreInsertColumns.Add(item.Name, t1Type.Name);
                        }
                    }
                }
            }
            return insertable;
        }
        public static IInsertable<T> IgnoreColumns<T, T1>(this IInsertable<T> insertable, Expression<Func<T, T1>> expression, bool where = true)
        {
            if (where)
            {
                Type t1Type = typeof(T1);
                PropertyInfo[] props = t1Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (props.Length > 0)
                {
                    foreach (var item in props)
                    {
                        if (!insertable.InsertBuilder.Context.IgnoreInsertColumns.Any(x => x.PropertyName == item.Name))
                        {
                            insertable.InsertBuilder.Context.IgnoreInsertColumns.Add(item.Name, t1Type.Name);
                        }
                    }
                }
            }
            return insertable;
        }
        public static MtInsertableProvider<T> IgnoreColumns<T, T1>(this MtInsertableProvider<T> insertable, T1 obj, bool where = true)
        {
            if (where)
            {
                Type t1Type = typeof(T1);
                PropertyInfo[] props = t1Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (props.Length > 0)
                {
                    foreach (var item in props)
                    {
                        if (!insertable.IgnoreColumns.Any(x => x.PropertyName == item.Name
                                                       || x.DbColumnName == item.Name))
                        {
                            insertable.IgnoreColumns.Add(new DbColumnInfo
                            {
                                DbColumnName = item.Name,
                                PropertyName = item.Name,
                                PropertyType = item.PropertyType
                            });
                        }

                    }
                }
            }
            return insertable;
        }
        public static MtInsertableProvider<T> IgnoreColumns<T, T1>(this MtInsertableProvider<T> insertable, Expression<Func<T, T1>> expression, bool where = true)
        {
            if (where)
            {
                Type t1Type = typeof(T1);
                PropertyInfo[] props = t1Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (props.Length > 0)
                {
                    foreach (var item in props)
                    {
                        if (!insertable.IgnoreColumns.Any(x => x.PropertyName == item.Name
                        || x.DbColumnName == item.Name))
                        {
                            insertable.IgnoreColumns.Add(new DbColumnInfo
                            {
                                PropertyName = item.Name,
                                DbColumnName = item.Name,
                                PropertyType = item.PropertyType
                            });
                        }
                    }
                }
            }
            return insertable;
        }
        #endregion

        #region InsertColumns
        public static IInsertable<T> InsertColumns<T, T1>(this IInsertable<T> insertable, T1 obj)
        {
            try
            {
                Type t1Type = typeof(T1);
                PropertyInfo[] props = t1Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (props.Length > 0)
                {
                    insertable.InsertBuilder.DbColumnInfoList.Clear();
                    foreach (var item in props)
                    {
                        DbColumnInfo dbColumn = new DbColumnInfo
                        {
                            DbColumnName = item.Name,
                            PropertyName = item.Name,
                            PropertyType = item.PropertyType
                        };
                        insertable.InsertBuilder.DbColumnInfoList.Add(dbColumn);
                    }
                }
            }
            catch
            {
            }
            return insertable;
        }

        public static MtInsertableProvider<T> InsertColumns<T, T1>(this MtInsertableProvider<T> insertable, T1 obj)
        {
            try
            {
                Type t1Type = typeof(T1);
                PropertyInfo[] props = t1Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (props.Length > 0)
                {
                    insertable.Columns.Clear();
                    foreach (var item in props)
                    {
                        DbColumnInfo dbColumn = new DbColumnInfo
                        {
                            DbColumnName = item.Name,
                            PropertyName = item.Name,
                            PropertyType = item.PropertyType
                        };
                        insertable.Columns.Add(dbColumn);
                    }
                }
            }
            catch
            {
            }
            return insertable;
        }

        public static IInsertable<T> InsertColumns<T, T1>(this IInsertable<T> insertable, Expression<Func<T, T1>> expression)
        {
            try
            {
                Type t1Type = typeof(T1);
                PropertyInfo[] props = t1Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (props.Length > 0)
                {
                    insertable.InsertBuilder.DbColumnInfoList.Clear();
                    foreach (var item in props)
                    {
                        DbColumnInfo dbColumn = new DbColumnInfo
                        {
                            DbColumnName = item.Name,
                            PropertyName = item.Name,
                            PropertyType = item.PropertyType
                        };
                        insertable.InsertBuilder.DbColumnInfoList.Add(dbColumn);
                    }
                }
            }
            catch
            {
            }
            return insertable;
        }

        public static MtInsertableProvider<T> InsertColumns<T, T1>(this MtInsertableProvider<T> insertable, Expression<Func<T, T1>> expression)
        {
            try
            {
                Type t1Type = typeof(T1);
                PropertyInfo[] props = t1Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (props.Length > 0)
                {
                    insertable.Columns.Clear();
                    foreach (var item in props)
                    {
                        DbColumnInfo dbColumn = new DbColumnInfo
                        {
                            DbColumnName = item.Name,
                            PropertyName = item.Name,
                            PropertyType = item.PropertyType
                        };
                        insertable.Columns.Add(dbColumn);
                    }
                }
            }
            catch
            {
            }
            return insertable;
        }

        public static MtInsertableProvider<T> Insertable<T>(this ISqlSugarClient client)
        {
            try
            {
                MtInsertableProvider<T> provider = new MtInsertableProvider<T>
                {
                    Ado = client.Ado
                };
                return provider;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static MtInsertableProvider<T> Insertable<T>(this ISqlSugarClient client, List<T> values)
        {
            try
            {
                MtInsertableProvider<T> provider = new MtInsertableProvider<T>
                {
                    InsertBy = InsertBy.ProvidedValues,
                    Ado = client.Ado,
                    Values = values
                };
                return provider;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static MtInsertableProvider<T> Insertable<T>(this ISqlSugarClient client, T value)
        {
            try
            {
                MtInsertableProvider<T> provider = new MtInsertableProvider<T>
                {
                    InsertBy = InsertBy.ProvidedValues,
                    Ado = client.Ado,
                    Values = new List<T> { value }
                };
                return provider;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static MtInsertableProvider<T> Insertable<T>(this SimpleClient<T> client) where T : class, new()
        {
            try
            {
                MtInsertableProvider<T> provider = new MtInsertableProvider<T>
                {
                    Ado = client.AsSugarClient().Ado
                };
                return provider;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static MtInsertableProvider<T> Insertable<T>(this SimpleClient<T> client, List<T> values) where T : class, new()
        {
            try
            {
                MtInsertableProvider<T> provider = new MtInsertableProvider<T>
                {
                    Ado = client.AsSugarClient().Ado,
                    InsertBy = InsertBy.ProvidedValues,
                    Values = values
                };
                return provider;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static MtInsertableProvider<T> ValuesFrom<T, T1>(this MtInsertableProvider<T> mtInsertableProvider, ISugarQueryable<T1> sugarQueryable)
        {
            try
            {
                mtInsertableProvider.InsertBy = InsertBy.SelectQuery;
                mtInsertableProvider.QueryBuilder = sugarQueryable.QueryBuilder;
                return mtInsertableProvider;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static MtInsertableProvider<T> ValuesFrom<T>(this MtInsertableProvider<T> mtInsertableProvider, List<T> providedValues)
        {
            try
            {
                mtInsertableProvider.InsertBy = InsertBy.ProvidedValues;
                mtInsertableProvider.Values = providedValues;
                return mtInsertableProvider;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static MtInsertableProvider<T> ValuesFrom<T>(this MtInsertableProvider<T> mtInsertableProvider, T providedValue)
        {
            try
            {
                mtInsertableProvider.InsertBy = InsertBy.ProvidedValues;
                mtInsertableProvider.Values = new List<T> { providedValue };
                return mtInsertableProvider;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region SmartExecAsync
        public async static Task<ExecResult> SmartExecAsync<T>(this IInsertable<T> iinsertable, bool where = true, ModifyType insertType = ModifyType.UpdateExisted) where T : class, new()
        {
            ExecResult execResult = new ExecResult();
            if (where)
            {
                InsertableProvider<T> insertable = iinsertable as InsertableProvider<T>;
                MtMysqlInsertBuilder<T> builder = new MtMysqlInsertBuilder<T>();
                insertable.InsertBuilder.DbColumnInfoList.RemoveAll(x => insertable.InsertBuilder.Context.IgnoreInsertColumns.Any(y => y.PropertyName == x.PropertyName));
                builder.Columns = insertable.InsertBuilder.DbColumnInfoList;
                builder.InsertedResultType = InsertedResultType.AffectedRowsCount;
                builder.InsertType = insertType;
                builder.Values = insertable.InsertObjs.ToList();
                execResult.Total = await insertable.Ado.ExecuteCommandAsync(builder.BuildSql(), builder.GetParameters().ToArray());
            }
            return execResult;
        }
        public async static Task<ExecReturnIdResult> SmartExecReturnIdAsync<T>(this IInsertable<T> iinsertable, bool where = true, ModifyType insertType = ModifyType.UpdateExisted) where T : class, new()
        {
            ExecReturnIdResult execReturnIdResult = new ExecReturnIdResult();
            if (where)
            {
                InsertableProvider<T> insertable = iinsertable as InsertableProvider<T>;
                MtMysqlInsertBuilder<T> builder = new MtMysqlInsertBuilder<T>();
                insertable.InsertBuilder.DbColumnInfoList.RemoveAll(x => insertable.InsertBuilder.Context.IgnoreInsertColumns.Any(y => y.PropertyName == x.PropertyName));
                builder.Columns = insertable.InsertBuilder.DbColumnInfoList;
                builder.InsertedResultType = InsertedResultType.AffectedLastId;
                builder.InsertType = insertType;
                builder.Values = insertable.InsertObjs.ToList();
                execReturnIdResult.ReturnId = await insertable.Ado.GetIntAsync(builder.BuildSql(), builder.GetParameters().ToArray());
            }
            return execReturnIdResult;
        }
        public async static Task<ExecResult> SmartExecAsync<T>(this MtInsertableProvider<T> insertable, bool where = true, ModifyType insertType = ModifyType.UpdateExisted) where T : class, new()
        {
            ExecResult execResult = new ExecResult();
            if (where)
            {
                MtMysqlInsertBuilder<T> builder = new MtMysqlInsertBuilder<T>
                {
                    Columns = insertable.Columns,
                    InsertedResultType = InsertedResultType.AffectedRowsCount,
                    InsertType = insertType,
                    Values = insertable.Values,
                    InsertBy = insertable.InsertBy,
                    QueryBuilder = insertable.QueryBuilder
                };
                execResult.Total = await insertable.Ado.ExecuteCommandAsync(builder.BuildSql(), builder.GetParameters().ToArray());
            }
            return execResult;
        }

        public async static Task<BatchExecReturnIdResult> SmartBatchExecReturnIdAsync<T>(this MtInsertableProvider<T> insertable, bool where = true, ModifyType insertType = ModifyType.UpdateExisted) where T : class, new()
        {
            BatchExecReturnIdResult batchExecReturnIdResult = new BatchExecReturnIdResult();
            if (where)
            {
                var needCreateTransaction = insertable.Ado.Transaction == null;
                try
                {
                    if (needCreateTransaction)
                    {
                        insertable.Ado.BeginTran();
                    }
                    MtMysqlInsertBuilder<T> builder = new MtMysqlInsertBuilder<T>
                    {
                        Columns = insertable.Columns,
                        InsertedResultType = InsertedResultType.AffectedAllId,
                        InsertType = insertType,
                        Values = insertable.Values,
                        InsertBy = insertable.InsertBy,
                        QueryBuilder = insertable.QueryBuilder
                    };
                    var exeResult = await insertable.Ado.GetDataTableAsync(builder.BuildSql(), builder.GetParameters().ToArray());
                    if (exeResult != null && exeResult.Rows.Count > 0)
                    {
                        if (int.TryParse(exeResult.Rows[0][0].ToString(), out int id) && id > 0)
                        {
                            batchExecReturnIdResult.ReturnIds = exeResult.AsEnumerable().Select(x => Convert.ToInt32(x[0])).ToList();
                        }
                        else
                        {
                            batchExecReturnIdResult.ReturnIds = null;
                        }
                    }
                    if (needCreateTransaction && insertable.Ado.Transaction != null)
                    {
                        insertable.Ado.CommitTran();
                    }
                }
                catch (Exception ex)
                {
                    if (needCreateTransaction && insertable.Ado.Transaction != null)
                    {
                        insertable.Ado.RollbackTran();
                    }
                    throw ex.GetInnnerException();
                }
            }
            return batchExecReturnIdResult;
        }

        public async static Task<BatchExecReturnEntityResult<T>> SmartBatchExecReturnEntityAsync<T>(this MtInsertableProvider<T> insertable, bool where = true, ModifyType insertType = ModifyType.UpdateExisted) where T : class, new()
        {
            BatchExecReturnEntityResult<T> batchExecReturnEntityResult = new BatchExecReturnEntityResult<T>();
            if (where)
            {
                var needCreateTransaction = insertable.Ado.Transaction == null;
                try
                {
                    if (needCreateTransaction)
                    {
                        insertable.Ado.BeginTran();
                    }
                    MtMysqlInsertBuilder<T> builder = new MtMysqlInsertBuilder<T>
                    {
                        Columns = insertable.Columns,
                        InsertedResultType = InsertedResultType.AffectedAllEntity,
                        InsertType = insertType,
                        Values = insertable.Values,
                        InsertBy = insertable.InsertBy,
                        QueryBuilder = insertable.QueryBuilder
                    };
                    var exeResult = await insertable.Ado.GetDataTableAsync(builder.BuildSql(), builder.GetParameters().ToArray());
                    if (exeResult != null && exeResult.Rows.Count > 0)
                    {
                        if (int.TryParse(exeResult.Rows[0][0].ToString(), out int id) && id <= 0)
                        {
                            batchExecReturnEntityResult.Entities = null;
                        }
                        else
                        {
                            batchExecReturnEntityResult.Entities = exeResult.ToEntity<T>();
                        }
                    }
                    if (needCreateTransaction && insertable.Ado.Transaction != null)
                    {
                        insertable.Ado.CommitTran();
                    }
                }
                catch (Exception ex)
                {
                    if (needCreateTransaction && insertable.Ado.Transaction != null)
                    {
                        insertable.Ado.RollbackTran();
                    }
                    throw ex.GetInnnerException();
                }
            }
            return batchExecReturnEntityResult;
        }

        public async static Task<ExecReturnIdResult> SmartExecReturnIdAsync<T>(this MtInsertableProvider<T> insertable, bool where = true, ModifyType insertType = ModifyType.UpdateExisted) where T : class, new()
        {
            ExecReturnIdResult execReturnIdResult = new ExecReturnIdResult();
            if (where)
            {
                MtMysqlInsertBuilder<T> builder = new MtMysqlInsertBuilder<T>
                {
                    Columns = insertable.Columns,
                    InsertedResultType = InsertedResultType.AffectedLastId,
                    InsertType = insertType,
                    Values = insertable.Values,
                    InsertBy = insertable.InsertBy,
                    QueryBuilder = insertable.QueryBuilder
                };
                execReturnIdResult.ReturnId = await insertable.Ado.GetIntAsync(builder.BuildSql(), builder.GetParameters().ToArray());
            }
            return execReturnIdResult;
        }

        #endregion

        #region SmartExec
        public static ExecResult SmartExec<T>(this IInsertable<T> iinsertable, bool where = true, ModifyType insertType = ModifyType.UpdateExisted) where T : class, new()
        {
            ExecResult execResult = new ExecResult();
            if (where)
            {
                InsertableProvider<T> insertable = iinsertable as InsertableProvider<T>;
                MtMysqlInsertBuilder<T> builder = new MtMysqlInsertBuilder<T>();
                insertable.InsertBuilder.DbColumnInfoList.RemoveAll(x => insertable.InsertBuilder.Context.IgnoreInsertColumns.Any(y => y.PropertyName == x.PropertyName));
                builder.Columns = insertable.InsertBuilder.DbColumnInfoList;
                builder.InsertedResultType = InsertedResultType.AffectedRowsCount;
                builder.InsertType = insertType;
                builder.Values = insertable.InsertObjs.ToList();
                execResult.Total = insertable.Ado.ExecuteCommand(builder.BuildSql(), builder.GetParameters().ToArray());
            }
            return execResult;
        }
        public static ExecReturnIdResult SmartExecReturnId<T>(this IInsertable<T> iinsertable, bool where = true, ModifyType insertType = ModifyType.UpdateExisted) where T : class, new()
        {
            ExecReturnIdResult execReturnIdResult = new ExecReturnIdResult();
            if (where)
            {
                InsertableProvider<T> insertable = iinsertable as InsertableProvider<T>;
                MtMysqlInsertBuilder<T> builder = new MtMysqlInsertBuilder<T>();
                insertable.InsertBuilder.DbColumnInfoList.RemoveAll(x => insertable.InsertBuilder.Context.IgnoreInsertColumns.Any(y => y.PropertyName == x.PropertyName));
                builder.Columns = insertable.InsertBuilder.DbColumnInfoList;
                builder.InsertedResultType = InsertedResultType.AffectedLastId;
                builder.InsertType = insertType;
                builder.Values = insertable.InsertObjs.ToList();
                execReturnIdResult.ReturnId = insertable.Ado.GetInt(builder.BuildSql(), builder.GetParameters().ToArray());
            }
            return execReturnIdResult;
        }
        public static ExecResult SmartExec<T>(this MtInsertableProvider<T> insertable, bool where = true, ModifyType insertType = ModifyType.UpdateExisted) where T : class, new()
        {
            ExecResult execResult = new ExecResult();
            if (where)
            {
                MtMysqlInsertBuilder<T> builder = new MtMysqlInsertBuilder<T>
                {
                    Columns = insertable.Columns,
                    InsertedResultType = InsertedResultType.AffectedRowsCount,
                    InsertType = insertType,
                    Values = insertable.Values,
                    InsertBy = insertable.InsertBy,
                    QueryBuilder = insertable.QueryBuilder
                };
                execResult.Total = insertable.Ado.ExecuteCommand(builder.BuildSql(), builder.GetParameters().ToArray());
            }
            return execResult;
        }
        public static ExecReturnIdResult SmartExecReturnId<T>(this MtInsertableProvider<T> insertable, bool where = true, ModifyType insertType = ModifyType.UpdateExisted) where T : class, new()
        {
            ExecReturnIdResult execReturnIdResult = new ExecReturnIdResult();
            if (where)
            {
                MtMysqlInsertBuilder<T> builder = new MtMysqlInsertBuilder<T>
                {
                    Columns = insertable.Columns,
                    InsertedResultType = InsertedResultType.AffectedLastId,
                    InsertType = insertType,
                    Values = insertable.Values,
                    InsertBy = insertable.InsertBy,
                    QueryBuilder = insertable.QueryBuilder
                };
                execReturnIdResult.ReturnId = insertable.Ado.GetInt(builder.BuildSql(), builder.GetParameters().ToArray());
            }
            return execReturnIdResult;
        }

        #endregion

        #region GroupConcat
        public static ISugarQueryable<T> GroupConcat<T, T1>(this ISugarQueryable<T> queryable, Expression<Func<T, T1>> expression, bool distinct = true, string separator = ",")
        {
            var paras = expression.Parameters;
            var body = expression.Body;
            string left = queryable.SqlBuilder.SqlTranslationLeft;
            string right = queryable.SqlBuilder.SqlTranslationRight;
            if (body is MemberExpression memberExpression)
            {
                string selectString = queryable.QueryBuilder.GetSelectValue;
                if (!string.IsNullOrWhiteSpace(selectString))
                {
                    string[] selectStringList = queryable.QueryBuilder.GetSelectValue.Split(",", StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < selectStringList.Length; i++)
                    {
                        string colName = $"{left}{memberExpression.Member.Name}{right}";
                        if (selectStringList[i].Contains("."))
                        {
                            colName = $"{left}{paras[0].Name}{right}.{colName}";
                        }
                        if (selectStringList[i].Contains(colName, StringComparison.OrdinalIgnoreCase))
                        {
                            selectStringList[i] = selectStringList[i].Replace(colName, $"GROUP_CONCAT({(distinct ? "distinct" : "")} {colName} SEPARATOR '{separator}') ", StringComparison.OrdinalIgnoreCase);
                        }
                    }
                    queryable.QueryBuilder.SelectValue = string.Join(",", selectStringList);
                }
            }
            return queryable;
        }

        #endregion

        #region Select
        public static ISugarQueryable<TGet> SmartSelect<T, TGet>(this ISugarQueryable<T> queryable, Expression<Func<T, TGet>> expression)
        {
            int paramLength = queryable.QueryBuilder.Parameters.Count;
            SqlResult sqlResult = SqlExtension.ExpressionToSelectSql(expression.Body, ref paramLength, "");
            queryable.QueryBuilder.SelectValue = sqlResult.SqlString.ToString();
            queryable.AddParameters(sqlResult.Params);
            ISugarQueryable<TGet> getQueryable = InstanceFactory.GetQueryable<TGet>(queryable.Context.CurrentConnectionConfig);
            getQueryable.Context = queryable.Context;
            getQueryable.SqlBuilder = queryable.SqlBuilder;
            getQueryable.SqlBuilder.QueryBuilder.Parameters = queryable.QueryBuilder.Parameters;
            getQueryable.SqlBuilder.QueryBuilder.SelectValue = queryable.QueryBuilder.SelectValue;
            return getQueryable;
        }
        #endregion
    }
}
