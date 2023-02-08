using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusiBotProd.Utilities.Data;
using MusiBotProd.Utilities.Data.DataProviders;

namespace MusiBot
{
    /// <summary>
    /// Dependency injection magic
    /// </summary>
    public class Startup
    {
        #region readonlies

        private readonly IConfigurationRoot _configuration;
        private readonly IDataProvider _dataProvider;            
    
        #endregion

        #region constructors

        /// <summary>
        /// Constructor, which fills our readonlies
        /// </summary>
        public Startup()
        {
            IConfigurationBuilder? configurationBuilder;

            string configFileName = @".\config.yml";           
            
            configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddYamlFile("config.yml");
            
            _configuration = configurationBuilder.Build();
            _dataProvider =
                new MySqlDataProvider(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING"));
        }

        #endregion

        #region supporting methods

        /// <summary>
        /// Static execution
        /// </summary>
        public static async Task ExecuteAsync()
        {
            Startup executor = new Startup();
            await executor.ExecuteObjectAsync();
        }

        /// <summary>
        /// Services configuration
        /// </summary>
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 1000,
                GatewayIntents = GatewayIntents.All,
            })).AddSingleton(new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose,
                DefaultRunMode = RunMode.Async
            }))
            .AddSingleton<Services.CommandsService>()
            .AddSingleton<Services.AdminService>()
            .AddSingleton<Services.LoggingService>()
            .AddSingleton<Services.InteractionsService>()
            .AddSingleton<Services.StartupService>()
            .AddSingleton(_configuration)
            .AddSingleton(_dataProvider);
        }

        /// <summary>
        /// Actual execution
        /// </summary>
        public async Task ExecuteObjectAsync()
        {       
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetRequiredService<Services.CommandsService>();
            serviceProvider.GetRequiredService<Services.AdminService>();
            serviceProvider.GetRequiredService<Services.LoggingService>();
            serviceProvider.GetRequiredService<Services.InteractionsService>();

            await serviceProvider.GetRequiredService<Services.StartupService>().ExecuteAsync();
            await Task.Delay(-1);
        }

        #endregion
    }
}
