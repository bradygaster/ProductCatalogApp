using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Web.Hosting;

namespace ProductCatalog
{
    public static class AppConfiguration
    {
        private static IConfiguration _configuration;
        private static readonly object _lock = new object();

        public static IConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    lock (_lock)
                    {
                        if (_configuration == null)
                        {
                            _configuration = BuildConfiguration();
                        }
                    }
                }
                return _configuration;
            }
        }

        private static IConfiguration BuildConfiguration()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var basePath = HostingEnvironment.MapPath("~/") ?? AppDomain.CurrentDomain.BaseDirectory;

            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }

        public static string GetAppSetting(string key)
        {
            return Configuration[$"AppSettings:{key}"];
        }

        public static string GetConnectionString(string name)
        {
            return Configuration.GetConnectionString(name);
        }
    }
}
