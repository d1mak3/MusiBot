using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using MusiBotProd.Utilities.Data;
using MusiBotProd.Utilities.Visuals;
using MusiBotProd.Utilities.Visuals.Buttons;

namespace MusiBot.Services
{
    /// <summary>
    /// Service to handle interactions
    /// </summary>
    public class InteractionsService
    {
        #region readonlies

        private readonly DiscordSocketClient _discordClient;
        private readonly IDataProvider _dataProvider;
        private readonly List<IButton> buttons = new List<IButton>();

        #endregion

        #region constructors

        public InteractionsService(
                DiscordSocketClient discordClient,
                IDataProvider dataProvider
            )
        {
            _discordClient = discordClient;
            _dataProvider = dataProvider;

            _discordClient.ButtonExecuted += ButtonsClicksHandler;

            BlackjackHitButton hitButton = new BlackjackHitButton();
            hitButton.DataProvider = _dataProvider;
            hitButton.DiscordClient = _discordClient;

            BlackjackStandButton standButton = new BlackjackStandButton();
            standButton.DataProvider = _dataProvider;
            standButton.DiscordClient = _discordClient;

            buttons.Add(hitButton);
            buttons.Add(standButton);
        }

        #endregion

        #region interaction handlers

        private async Task ButtonsClicksHandler(SocketMessageComponent component)
        {
            foreach (BlackjackButton button in buttons)
            {
                if (component.Data.CustomId == button.CustomId)
                {
                    button.MessageComponent = component;
                    await button.ExecuteAsync();
                }
            }
        }

        #endregion
    }
}
