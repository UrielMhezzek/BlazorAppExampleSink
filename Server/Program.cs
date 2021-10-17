using BlazorAppExampleSink.Server;
using BlazorAppExampleSink.Server.Extentions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorAppExampleSink
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateLogger();

            try
            {
                Log.Information("Starting web host");

                CreateHostBuilder(args)
                    .UseSerilog()
                    .Build()
                    .MigrateDatabase()
                    .Run();

#if DEBUG
                Process.Start("https://localhost:5001/");
#endif
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static void CreateLogger()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var isDevelopment = environment == Environments.Development;
            IConfigurationRoot _config = GetConfig(environment);

            var db = _config.GetValue<string>("ConnectionStrings:DefaultConnection");

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Debug()
                .WriteTo.Seq("http://localhost:5341")
                .WriteTo.File("Logs\\log.txt", rollingInterval: RollingInterval.Day)
                .WriteTo.MSSqlServer(
                    connectionString: db,
                    sinkOptions: GetSinkOptions(),
                    columnOptions: GetColumnOptions()
                )
                .CreateLogger();
        }

        private static IConfigurationRoot GetConfig(string environment)
        {
            return new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{environment}.json", optional: true)
                            .AddEnvironmentVariables()
                            .Build();
        }

        private static ColumnOptions GetColumnOptions()
        {
            var columnOpts = new ColumnOptions();
            columnOpts.Store.Remove(StandardColumn.Properties);
            columnOpts.Store.Add(StandardColumn.LogEvent);
            columnOpts.LogEvent.DataLength = 2048;
            columnOpts.PrimaryKey = columnOpts.TimeStamp;
            columnOpts.TimeStamp.NonClusteredIndex = true;
            return columnOpts;
        }


        private static MSSqlServerSinkOptions GetSinkOptions() => new MSSqlServerSinkOptions()
        {
            TableName = "Log",
            AutoCreateSqlTable = true,
            SchemaName = "Logs"
        };

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
