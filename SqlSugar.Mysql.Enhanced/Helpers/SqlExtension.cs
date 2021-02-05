using Sqsugar.Mysql.Enhanced.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Sqsugar.Mysql.Enhanced.Helpers
{
    public static class SqlExtension
    {
        /// <summary>
        /// 根据表达式生成where的sql
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="paramLength">多一个参数，该数字+1,这样可以保证sql参数名的唯一性</param>
        /// <param name="preParam">参数名前缀</param>
        /// <returns></returns>
        public static SqlResult ExpressionToWhereSql(this System.Linq.Expressions.Expression expression, ref int paramLength, string preParam = "@")
        {
            SqlResult sqlResult = new SqlResult();
            if (expression is BinaryExpression binaryExpression)
            {
                string symbol = string.Empty;
                if (expression.NodeType == ExpressionType.AndAlso)
                {
                    symbol = "AND";
                }
                else if (expression.NodeType == ExpressionType.OrElse)
                {
                    symbol = "OR";
                }
                else if (expression.NodeType == ExpressionType.Equal)
                {
                    symbol = "=";
                }
                else if (expression.NodeType == ExpressionType.NotEqual)
                {
                    symbol = "!=";
                }
                else if (expression.NodeType == ExpressionType.Add)
                {
                    symbol = "+";
                }
                else if (expression.NodeType == ExpressionType.Subtract)
                {
                    symbol = "-";
                }
                else if (expression.NodeType == ExpressionType.Multiply)
                {
                    symbol = "*";
                }
                else if (expression.NodeType == ExpressionType.Divide)
                {
                    symbol = "/";
                }
                else if (expression.NodeType == ExpressionType.Modulo)
                {
                    symbol = "%";
                }
                else if (expression.NodeType == ExpressionType.And)
                {
                    symbol = "&";
                }
                else if (expression.NodeType == ExpressionType.Or)
                {
                    symbol = "|";
                }
                else if (expression.NodeType == ExpressionType.ExclusiveOr)
                {
                    symbol = "^";
                }
                else if (expression.NodeType == ExpressionType.LeftShift)
                {
                    symbol = "<<";
                }
                else if (expression.NodeType == ExpressionType.RightShift)
                {
                    symbol = ">>";
                }
                else if (expression.NodeType == ExpressionType.LessThan)
                {
                    symbol = "<";
                }
                else if (expression.NodeType == ExpressionType.GreaterThan)
                {
                    symbol = ">";
                }
                else if (expression.NodeType == ExpressionType.LessThanOrEqual)
                {
                    symbol = "<=";
                }
                else if (expression.NodeType == ExpressionType.GreaterThanOrEqual)
                {
                    symbol = ">=";
                }
                if (symbol != string.Empty)
                {
                    SqlResult leftSqlResult = ExpressionToWhereSql(binaryExpression.Left, ref paramLength, preParam);
                    SqlResult rightSqlResult = ExpressionToWhereSql(binaryExpression.Right, ref paramLength, preParam);
                    if (leftSqlResult.SqlString.ToString() == "null" && rightSqlResult.SqlString.ToString() != "null")
                    {
                        symbol = "IS";
                        sqlResult.SqlString.Append($"({rightSqlResult.SqlString} {symbol} {leftSqlResult.SqlString})");
                        foreach (var param in rightSqlResult.Params)
                        {
                            sqlResult.Params.TryAdd(param.Key, param.Value);
                        }
                    }
                    else if (leftSqlResult.SqlString.ToString() != "null" && rightSqlResult.SqlString.ToString() == "null")
                    {
                        symbol = "IS";
                        sqlResult.SqlString.Append($"({leftSqlResult.SqlString} {symbol} {rightSqlResult.SqlString})");
                        foreach (var param in leftSqlResult.Params)
                        {
                            sqlResult.Params.TryAdd(param.Key, param.Value);
                        }
                    }
                    else if (leftSqlResult.SqlString.ToString() != "null" && rightSqlResult.SqlString.ToString() != "null")
                    {
                        sqlResult.SqlString.Append($"({leftSqlResult.SqlString} {symbol} {rightSqlResult.SqlString})");
                        foreach (var param in leftSqlResult.Params)
                        {
                            sqlResult.Params.TryAdd(param.Key, param.Value);
                        }
                        foreach (var param in rightSqlResult.Params)
                        {
                            sqlResult.Params.TryAdd(param.Key, param.Value);
                        }
                    }
                    else
                    {
                        sqlResult.SqlString.Append($"(1 = 1)");
                    }
                }
            }
            else if (expression is UnaryExpression unaryExpression)
            {
                if (unaryExpression.NodeType == ExpressionType.Convert)
                {
                    SqlResult convertResult = ExpressionToWhereSql(unaryExpression.Operand, ref paramLength, preParam);
                    sqlResult.SqlString.Append(convertResult.SqlString.ToString());
                    foreach (var param in convertResult.Params)
                    {
                        sqlResult.Params.TryAdd(param.Key, param.Value);
                    }
                }
            }
            else if (expression is ConstantExpression constantExpression)
            {
                object value = constantExpression.Value;
                string paramName = $"{preParam}mtconst{++paramLength}";
                sqlResult.SqlString.Append($"{(preParam.StartsWith("@") ? "" : "@")}{paramName}");
                sqlResult.Params.TryAdd(paramName, value);
            }
            else if (expression is MemberExpression memberExpression)
            {
                if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
                {
                    sqlResult.SqlString.Append($"`{memberExpression.Member.Name}`");
                }
                else
                {
                    var obj = Expression.Lambda(memberExpression).Compile().DynamicInvoke();
                    if (obj == null)
                    {
                        sqlResult.SqlString.Append("null");
                    }
                    else
                    {
                        string paramName = $"{preParam}mtconst{++paramLength}";
                        sqlResult.SqlString.Append($"{(preParam.StartsWith("@") ? "" : "@")}{paramName}");
                        sqlResult.Params.TryAdd(paramName, obj);
                    }
                }
            }
            else if (expression is MethodCallExpression methodCallExpression)
            {
                Type stringType = typeof(String);
                List<string> argStrings = new List<string>();
                foreach (var arg in methodCallExpression.Arguments)
                {
                    SqlResult argResult = ExpressionToWhereSql(arg, ref paramLength, preParam);
                    foreach (var param in argResult.Params)
                    {
                        sqlResult.Params.TryAdd(param.Key, param.Value);
                    }
                    argStrings.Add(argResult.SqlString.ToString());
                }
                if (methodCallExpression.Method.ReflectedType == stringType)
                {
                    if (methodCallExpression.Method.Name == nameof(String.Concat))
                    {
                        sqlResult.SqlString.Append($"CONCAT({string.Join(",", argStrings)})");
                    }
                    else if (methodCallExpression.Method.Name == nameof(String.Contains))
                    {
                        SqlResult objResult = ExpressionToWhereSql(methodCallExpression.Object, ref paramLength, preParam);
                        sqlResult.SqlString.Append($"INSTR({objResult.SqlString},{argStrings.FirstOrDefault()})");
                        foreach (var param in objResult.Params)
                        {
                            sqlResult.Params.TryAdd(param.Key, param.Value);
                        }
                    }
                    else if (methodCallExpression.Method.Name == nameof(String.Equals))
                    {
                        SqlResult objResult = ExpressionToWhereSql(methodCallExpression.Object, ref paramLength, preParam);
                        sqlResult.SqlString.Append($"({objResult.SqlString}={argStrings.FirstOrDefault()})");
                        foreach (var param in objResult.Params)
                        {
                            sqlResult.Params.TryAdd(param.Key, param.Value);
                        }
                    }
                    else if (methodCallExpression.Method.Name == nameof(String.StartsWith))
                    {
                        SqlResult objResult = ExpressionToWhereSql(methodCallExpression.Object, ref paramLength, preParam);
                        sqlResult.SqlString.Append($"{objResult.SqlString} like {argStrings.FirstOrDefault()}%");
                        foreach (var param in objResult.Params)
                        {
                            sqlResult.Params.TryAdd(param.Key, param.Value);
                        }
                    }
                    else if (methodCallExpression.Method.Name == nameof(String.EndsWith))
                    {
                        SqlResult objResult = ExpressionToWhereSql(methodCallExpression.Object, ref paramLength, preParam);
                        sqlResult.SqlString.Append($"{objResult.SqlString} like %{argStrings.FirstOrDefault()}");
                        foreach (var param in objResult.Params)
                        {
                            sqlResult.Params.TryAdd(param.Key, param.Value);
                        }
                    }
                    else if (methodCallExpression.Method.Name == nameof(String.IsNullOrEmpty))
                    {
                        sqlResult.SqlString.Append($"({argStrings.FirstOrDefault()} is null or {argStrings.FirstOrDefault()} = '')");
                    }
                    else if (methodCallExpression.Method.Name == nameof(String.IsNullOrWhiteSpace))
                    {
                        sqlResult.SqlString.Append($"({argStrings.FirstOrDefault()} is null or trim({argStrings.FirstOrDefault()})='' )");
                    }
                    else if (methodCallExpression.Method.Name == nameof(String.Trim))
                    {
                        SqlResult objResult = ExpressionToWhereSql(methodCallExpression.Object, ref paramLength, preParam);
                        sqlResult.SqlString.Append($"trim({objResult.SqlString})");
                        foreach (var param in objResult.Params)
                        {
                            sqlResult.Params.TryAdd(param.Key, param.Value);
                        }
                    }
                    else if (methodCallExpression.Method.Name == nameof(String.TrimStart))
                    {
                        SqlResult objResult = ExpressionToWhereSql(methodCallExpression.Object, ref paramLength, preParam);
                        sqlResult.SqlString.Append($"ltrim({objResult.SqlString})");
                        foreach (var param in objResult.Params)
                        {
                            sqlResult.Params.TryAdd(param.Key, param.Value);
                        }
                    }
                    else if (methodCallExpression.Method.Name == nameof(String.TrimEnd))
                    {
                        SqlResult objResult = ExpressionToWhereSql(methodCallExpression.Object, ref paramLength, preParam);
                        sqlResult.SqlString.Append($"rtrim({objResult.SqlString})");
                        foreach (var param in objResult.Params)
                        {
                            sqlResult.Params.TryAdd(param.Key, param.Value);
                        }
                    }
                }
            }
            return sqlResult;
        }

        /// <summary>
        ///  根据表达式生成Select的sql
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="paramLength">多一个参数，该数字+1,这样可以保证sql参数名的唯一性</param>
        /// <param name="preParam">参数名前缀</param>
        /// <returns></returns>
        public static SqlResult ExpressionToSelectSql(this System.Linq.Expressions.Expression expression, ref int paramLength, string preParam = "@")
        {
            SqlResult sqlResult = new SqlResult();
            if (expression is ParameterExpression parameterExpression)
            {
                sqlResult.SqlString.Append(string.Join(",", parameterExpression.Type.GetProperties().Select(x => x.Name)));
            }
            else if (expression is ConstantExpression constantExpression)
            {
                if (constantExpression.Value == null)
                {
                    sqlResult.SqlString.Append("null");
                }
                else
                {
                    string paramName = $"{preParam}mtconst{++paramLength}";
                    sqlResult.SqlString.Append($"{(preParam.StartsWith("@") ? "" : "@")}{paramName}");
                    sqlResult.Params.TryAdd(paramName, constantExpression.Value);
                }
            }
            else if (expression is MemberExpression memberExpression)
            {
                if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
                {
                    sqlResult.SqlString.Append($"`{memberExpression.Member.Name}`");
                }
                else
                {
                    var obj = Expression.Lambda(memberExpression).Compile().DynamicInvoke();
                    if (obj == null)
                    {
                        sqlResult.SqlString.Append("null");
                    }
                    else
                    {
                        string paramName = $"{preParam}mtconst{++paramLength}";
                        sqlResult.SqlString.Append($"{(preParam.StartsWith("@") ? "" : "@")}{paramName}");
                        sqlResult.Params.TryAdd(paramName, obj);
                    }
                }
            }
            else if (expression is BinaryExpression binaryExpression)
            {
                string symbol = string.Empty;
                if (expression.NodeType == ExpressionType.AndAlso)
                {
                    symbol = "AND";
                }
                else if (expression.NodeType == ExpressionType.OrElse)
                {
                    symbol = "OR";
                }
                else if (expression.NodeType == ExpressionType.Equal)
                {
                    symbol = "=";
                }
                else if (expression.NodeType == ExpressionType.NotEqual)
                {
                    symbol = "!=";
                }
                else if (expression.NodeType == ExpressionType.Add)
                {
                    symbol = "+";
                }
                else if (expression.NodeType == ExpressionType.Subtract)
                {
                    symbol = "-";
                }
                else if (expression.NodeType == ExpressionType.Multiply)
                {
                    symbol = "*";
                }
                else if (expression.NodeType == ExpressionType.Divide)
                {
                    symbol = "/";
                }
                else if (expression.NodeType == ExpressionType.Modulo)
                {
                    symbol = "%";
                }
                else if (expression.NodeType == ExpressionType.And)
                {
                    symbol = "&";
                }
                else if (expression.NodeType == ExpressionType.Or)
                {
                    symbol = "|";
                }
                else if (expression.NodeType == ExpressionType.ExclusiveOr)
                {
                    symbol = "^";
                }
                else if (expression.NodeType == ExpressionType.LeftShift)
                {
                    symbol = "<<";
                }
                else if (expression.NodeType == ExpressionType.RightShift)
                {
                    symbol = ">>";
                }
                else if (expression.NodeType == ExpressionType.LessThan)
                {
                    symbol = "<";
                }
                else if (expression.NodeType == ExpressionType.GreaterThan)
                {
                    symbol = ">";
                }
                else if (expression.NodeType == ExpressionType.LessThanOrEqual)
                {
                    symbol = "<=";
                }
                else if (expression.NodeType == ExpressionType.GreaterThanOrEqual)
                {
                    symbol = ">=";
                }
                if (symbol != string.Empty)
                {
                    SqlResult leftSqlResult = ExpressionToSelectSql(binaryExpression.Left, ref paramLength, preParam);
                    SqlResult rightSqlResult = ExpressionToSelectSql(binaryExpression.Right, ref paramLength, preParam);
                    if (leftSqlResult.SqlString.ToString() == "null" && rightSqlResult.SqlString.ToString() != "null")
                    {
                        symbol = "IS";
                        sqlResult.SqlString.Append($"({rightSqlResult.SqlString} {symbol} {leftSqlResult.SqlString})");
                        foreach (var param in rightSqlResult.Params)
                        {
                            sqlResult.Params.TryAdd(param.Key, param.Value);
                        }
                    }
                    else if (leftSqlResult.SqlString.ToString() != "null" && rightSqlResult.SqlString.ToString() == "null")
                    {
                        symbol = "IS";
                        sqlResult.SqlString.Append($"({leftSqlResult.SqlString} {symbol} {rightSqlResult.SqlString})");
                        foreach (var param in leftSqlResult.Params)
                        {
                            sqlResult.Params.TryAdd(param.Key, param.Value);
                        }
                    }
                    else if (leftSqlResult.SqlString.ToString() != "null" && rightSqlResult.SqlString.ToString() != "null")
                    {
                        sqlResult.SqlString.Append($"({leftSqlResult.SqlString} {symbol} {rightSqlResult.SqlString})");
                        foreach (var param in leftSqlResult.Params)
                        {
                            sqlResult.Params.TryAdd(param.Key, param.Value);
                        }
                        foreach (var param in rightSqlResult.Params)
                        {
                            sqlResult.Params.TryAdd(param.Key, param.Value);
                        }
                    }
                    else
                    {
                        sqlResult.SqlString.Append($"(1 = 1)");
                    }
                }
            }
            else if (expression is UnaryExpression unaryExpression)
            {
                if (unaryExpression.NodeType == ExpressionType.Convert)
                {
                    SqlResult convertResult = ExpressionToSelectSql(unaryExpression.Operand, ref paramLength, preParam);
                    sqlResult.SqlString.Append(convertResult.SqlString.ToString());
                    foreach (var param in convertResult.Params)
                    {
                        sqlResult.Params.TryAdd(param.Key, param.Value);
                    }
                }
            }
            else if (expression is NewArrayExpression newArrayExpression)
            {
                foreach (Expression itemExp in newArrayExpression.Expressions)
                {
                    SqlResult itemExpResult = ExpressionToSelectSql(itemExp, ref paramLength, preParam);
                    sqlResult.SqlString.Append(itemExpResult.SqlString.ToString());
                    foreach (var item in itemExpResult.Params)
                    {
                        sqlResult.Params.TryAdd(item.Key, item.Value);
                    }
                }
            }
            else if (expression is MethodCallExpression methodCallExpression)
            {
                Type stringType = typeof(String);
                List<string> argStrings = new List<string>();
                foreach (var arg in methodCallExpression.Arguments)
                {
                    SqlResult argResult = ExpressionToSelectSql(arg, ref paramLength, preParam);
                    foreach (var param in argResult.Params)
                    {
                        sqlResult.Params.TryAdd(param.Key, param.Value);
                    }
                    argStrings.Add(argResult.SqlString.ToString());
                }
                if (methodCallExpression.Method.ReflectedType == stringType)
                {
                    if (methodCallExpression.Method.Name == nameof(String.IndexOf))
                    {
                        SqlResult objResult = ExpressionToSelectSql(methodCallExpression.Object, ref paramLength, preParam);
                        sqlResult.SqlString.Append($"INSTR({objResult.SqlString},{argStrings.FirstOrDefault()})");
                        foreach (var param in objResult.Params)
                        {
                            sqlResult.Params.TryAdd(param.Key, param.Value);
                        }
                    }
                    else if (methodCallExpression.Method.Name == nameof(String.Concat))
                    {
                        if (methodCallExpression.Arguments.Count == 1)
                        {
                            Expression firstArgExp = methodCallExpression.Arguments[0];
                            if (methodCallExpression.Arguments[0] is NewArrayExpression newArrayExpression2)
                            {
                                if (newArrayExpression2.Expressions.Count == 1)
                                {
                                    firstArgExp = newArrayExpression2.Expressions[0];
                                    if (firstArgExp is MethodCallExpression methodCallExpression1
                            && methodCallExpression1.Method.Name == nameof(string.Join)
                            && methodCallExpression1.Method.ReflectedType == stringType)
                                    {
                                        if (methodCallExpression1.Arguments[1] is NewArrayExpression newArrayExpression1
                                         && ((newArrayExpression1.Expressions[0] is MemberExpression memberExpression1
                                         && memberExpression1.Expression.NodeType == ExpressionType.Parameter) || (newArrayExpression1.Expressions[0].NodeType == ExpressionType.Call)))
                                        {
                                            SqlResult separatorResult = ExpressionToSelectSql(methodCallExpression1.Arguments[0], ref paramLength, preParam);
                                            SqlResult objResult = ExpressionToSelectSql(methodCallExpression1.Arguments[1], ref paramLength, preParam);
                                            sqlResult.SqlString.Append($"group_concat({objResult.SqlString} separator {separatorResult.SqlString})");
                                            foreach (var param in separatorResult.Params)
                                            {
                                                sqlResult.Params.TryAdd(param.Key, param.Value);
                                            }
                                            foreach (var param in objResult.Params)
                                            {
                                                sqlResult.Params.TryAdd(param.Key, param.Value);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (firstArgExp is MemberExpression memberExpression1
                                         && memberExpression1.Expression.NodeType == ExpressionType.Parameter)
                                        {
                                            SqlResult objResult = ExpressionToSelectSql(firstArgExp, ref paramLength, preParam);
                                            sqlResult.SqlString.Append($"group_concat({objResult.SqlString} separator ',')");
                                            foreach (var param in objResult.Params)
                                            {
                                                sqlResult.Params.TryAdd(param.Key, param.Value);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    List<string> concatItems = new List<string>();
                                    foreach (var itemExpression in newArrayExpression2.Expressions)
                                    {
                                        SqlResult itemExpressionResult = ExpressionToSelectSql(itemExpression, ref paramLength, preParam);
                                        foreach (var param in itemExpressionResult.Params)
                                        {
                                            sqlResult.Params.TryAdd(param.Key, param.Value);
                                        }
                                        concatItems.Add(itemExpressionResult.SqlString.ToString());
                                    }
                                    sqlResult.SqlString.Append($"CONCAT({string.Join(",", concatItems)})");
                                }
                            }
                        }
                        else
                        {
                            sqlResult.SqlString.Append($"CONCAT({string.Join(",", argStrings)})");
                        }
                    }
                }
                else if (methodCallExpression.Method.ReflectedType == typeof(Enumerable))
                {
                    if (methodCallExpression.Method.Name == nameof(Enumerable.Distinct))
                    {
                        SqlResult objResult = ExpressionToSelectSql(methodCallExpression.Arguments[0], ref paramLength, preParam);
                        if (objResult.SqlString.ToString().Contains("group_concat(", StringComparison.OrdinalIgnoreCase))
                        {
                            sqlResult.SqlString.Append(objResult.SqlString.Replace("group_concat(", "group_concat(distinct "));
                        }
                        else
                        {
                            sqlResult.SqlString.Append($"distinct({objResult.SqlString})");
                        }
                        foreach (var param in objResult.Params)
                        {
                            sqlResult.Params.TryAdd(param.Key, param.Value);
                        }
                    }
                }
                else if (methodCallExpression.Method.ReflectedType == typeof(object))
                {
                    if (methodCallExpression.Method.Name == nameof(object.ToString))
                    {
                        SqlResult objResult = ExpressionToSelectSql(methodCallExpression.Object, ref paramLength, preParam);
                        sqlResult.SqlString.Append(objResult.SqlString.ToString());
                        foreach (var param in objResult.Params)
                        {
                            sqlResult.Params.TryAdd(param.Key, param.Value);
                        }
                    }
                }
            }
            else if (expression is NewExpression newExpression)
            {
                List<string> items = new List<string>();
                for (int i = 0; i < newExpression.Arguments.Count; i++)
                {
                    var arg = newExpression.Arguments[i];
                    SqlResult argResult = ExpressionToSelectSql(arg, ref paramLength, preParam);
                    foreach (var item in argResult.Params)
                    {
                        sqlResult.Params.TryAdd(item.Key, item.Value);
                    }
                    if (arg.NodeType == ExpressionType.Parameter)
                    {
                        items.Add(argResult.SqlString.ToString());
                    }
                    else
                    {
                        items.Add($"{argResult.SqlString} AS {newExpression.Members[i].Name}");
                    }
                }
                sqlResult.SqlString.Append(string.Join(",", items));
            }
            return sqlResult;
        }
    }
}
