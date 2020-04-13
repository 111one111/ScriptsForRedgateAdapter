using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScriptsForRedgateAdapter.Business;
using ScriptsForRedgateAdapter.DAL;
using ScriptsForRedgateAdapter.Interfaces.Business;
using ScriptsForRedgateAdapter.Interfaces.Console;
using ScriptsForRedgateAdapter.Interfaces.DAL;
using ScriptsForRedgateAdapter.Models.Common;

namespace ScriptsForRedgateAdapter
{
    public class Startup
    {
        private IConfiguration _configuration { get; }
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
 
        public ServiceCollection ConfigureServices(ServiceCollection services, string ticketNumber)
        {
            services.AddTransient(typeof(IFileAccess<>), typeof(FileAccess<>));
            services.AddTransient<IScriptCheck, ScriptCheck>();
            services.AddTransient<IScriptsForRedgateAdapter, ScriptsForRedgateAdapter>();
            services.AddTransient<IProcessTemplate, ProcessTemplate>();
            services.AddTransient<IProcessRules, ProcessRules>();

            services.AddOptions();            
            services.Configure<AppConfig>(options => { 
                _configuration.GetSection(nameof(AppConfig)).Bind(options);
                options.TicketNumber = ticketNumber;
            });
            return services;
        }
    }
}
