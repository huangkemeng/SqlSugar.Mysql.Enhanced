using System.Collections.Generic;
using System.Text;

namespace Sqsugar.Mysql.Enhanced.Models
{
    public class SqlResult
    {

        /// <summary>
        /// 参数名和参数值
        /// </summary>
        public Dictionary<string, object> Params { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// 对应的sql字符串
        /// </summary>
        public StringBuilder SqlString { get; set; } = new StringBuilder();
    }
}
