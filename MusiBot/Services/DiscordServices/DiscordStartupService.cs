using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace MusiBotProd.Services.DiscordServices
{
    public class DiscordStartupService
    {
        #region readonlies

        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordSocketClient _discordClient;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _configuration;

        #endregion

        #region constructors

        /// <summary>
        /// Auto-completed by dependency injection constructor 
        /// </summary>
        public DiscordStartupService(
            IServiceProvider serviceProvider,
            DiscordSocketClient discordClient,
            CommandService commands,
            IConfigurationRoot configuration
            )
        {
            _serviceProvider = serviceProvider;
            _discordClient = discordClient;
            _commands = commands;
            _configuration = configuration;
        }

        #endregion

        #region supporting methods

        /// <summary>
        /// Executes the bot
        /// </summary>        
        public async Task ExecuteAsync()
        {
            string token = _configuration["token"];

            if (string.IsNullOrWhiteSpace(token)) throw new Exception("Invalid token");

            await _discordClient.LoginAsync(Discord.TokenType.Bot, token);
            await _discordClient.StartAsync();

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }

        #endregion
    }
}
