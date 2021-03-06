namespace SlackAlertOwner.Notifier
{
    using IoC;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            if (Debugger.IsAttached || ((IList) args).Contains("--debug"))
            {
                CreateHostBuilder(args).Build().Run();
            }
            else
            {
                var path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location ??
                                                 throw new Exception("Cannot resolve service location."));
                Directory.SetCurrentDirectory(path);
                CreateHostBuilder(args).Build().Run();
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var builder = new ConfigurationBuilder()
                        .SetBasePath(hostContext.HostingEnvironment.ContentRootPath)
                        .AddCommandLine(args)
                        .AddEnvironmentVariables()
                        .AddJsonFile("appsettings.json", false, true);

                    services.AddOptionsPattern(builder);
                    services.AddQuartz();
                    services.AddHttpClients();
                    services.AddEnvironment();
                    services.AddHostedService<QuartzHostedService>();
                })
                .UseSerilog();
    }
}