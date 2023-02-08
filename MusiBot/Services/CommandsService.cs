using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace MusiBot.Services
{
    /// <summary>
    /// Service to manage commands
    /// </summary>
    public class CommandsService
    {
        #region readonlies

        private readonly DiscordSocketClient _discordClient;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _configuration;
        private readonly IServiceProvider _serviceProvider;

        #endregion

        #region constructors

        /// <summary>
        /// Auto-completed by dependency injection constructor 
        /// </summary>
        public CommandsService(
            DiscordSocketClient discordClient,
            CommandService commands,
            IConfigurationRoot configuration,
            IServiceProvider serviceProvider)
        {
            _discordClient = discordClient;
            _commands = commands;
            _configuration = configuration;
            _serviceProvider = serviceProvider;

            _discordClient.MessageReceived += OnCommandReceivedAsync;
        }

        #endregion

        #region event handlers

        /// <summary>
        /// Execute command, when MessageReceived was triggered
        /// </summary>        
        private async Task OnCommandReceivedAsync(SocketMessage socketMessage)
        {
            SocketUserMessage? message = socketMessage as SocketUserMessage;

            if (message == null) return;
            if (message.Author.Id == _discordClient.CurrentUser.Id) return;

            SocketCommandContext commandContext = new SocketCommandContext(_discordClient, message);

            int index = 0;            
            if (message.HasStringPrefix(_configuration["prefix"], ref index) ||
                message.HasMentionPrefix(_discordClient.CurrentUser, ref index))
            {
                IResult result = await _commands.ExecuteAsync(commandContext, index, _serviceProvider);

                if (!result.IsSuccess)
                {
                    await commandContext.Channel.SendMessageAsync(result.ToString());
                }
            }
        }

        #endregion
    }
}
