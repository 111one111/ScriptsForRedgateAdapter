using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScriptsForRedgateAdapter.Interfaces.Console;
using System;
using System.IO;

namespace ScriptsForRedgateAdapter
{
    class Program
    {
        static IServiceProvider _serviceProvider;
        static void Main(string[] args)
        {
            string ticketNumber = CheckASrgsForTicketNumber(args);
            if(ticketNumber == string.Empty)
            {
                Console.ReadLine();
                return;
            }

            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            Startup setupApplication = new Startup(configuration);            
            _serviceProvider = setupApplication.ConfigureServices(new ServiceCollection(), ticketNumber)
                .BuildServiceProvider(true);

            IServiceScope scope = _serviceProvider.CreateScope();
            scope.ServiceProvider.GetRequiredService<IScriptsForRedgateAdapter>().Start();
            DisposeServices();          
        }

        private static string CheckASrgsForTicketNumber(string[] args)
        {
            if(args.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Need A ticket number to proceed");
                Console.ResetColor();
                return string.Empty;
            }

            return args[0];
        }

        private static void DisposeServices()
        {
            if (_serviceProvider == null)
            {
                return;
            }
            if (_serviceProvider is IDisposable)
            {
                ((IDisposable)_serviceProvider).Dispose();
            }
        }
    }
}
