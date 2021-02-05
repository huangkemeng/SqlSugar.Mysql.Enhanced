using System.Collections.Generic;

namespace Sqsugar.Mysql.Enhanced.Models
{
    public class BatchExecReturnEntityResult<T>
    {
        public List<T> Entities { get; set; }
    }
}
