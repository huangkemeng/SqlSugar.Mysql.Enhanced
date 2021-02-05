using Microsoft.AspNetCore.Builder;
using System;

namespace Sqsugar.Mysql.Enhanced.Helpers
{
    public static class ServiceAccessor
    {
        private static IServiceProvider serviceProvider;
        public static IApplicationBuilder UseServiceAccessor(this IApplicationBuilder app)
        {
            serviceProvider = app.ApplicationServices;
            return app;
        }
        public static T Get<T>()
        {
            var service = serviceProvider.GetService(typeof(T));
            if (service == null)
            {
                throw new InvalidOperationException($"Please Use Type {typeof(T).Name} First");
            }
            return (T)service;
        }

        public static object Get(Type type)
        {
            var service = serviceProvider.GetService(type);
            if (service == null)
            {
                throw new InvalidOperationException($"Please Use Type {type.Name} First");
            }
            return service;
        }
    }
}
