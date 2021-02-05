using System;

namespace Sqsugar.Mysql.Enhanced.Helpers
{
    public static class ExceptionExtension
    {
        public static Exception GetInnnerException(this Exception ex)
        {
            if (ex.InnerException == null) return ex;
            return ex.InnerException.GetInnnerException();
        }
    }
}
