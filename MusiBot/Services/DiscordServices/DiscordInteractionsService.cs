using Discord.WebSocket;
using MusiBotProd.Utilities.Data;
using MusiBotProd.Utilities.Visuals;
using MusiBotProd.Utilities.Visuals.DiscordButtons;

namespace MusiBotProd.Services.DiscordServices
{
    /// <summary>
    /// Service to handle interactions
    /// </summary>
    public class DiscordInteractionsService
    {
        #region readonlies

        private readonly DiscordSocketClient _discordClient;
        private readonly IDataProvider _dataProvider;
        private readonly List<IButton> buttons = new List<IButton>();

        #endregion

        #region constructors

        public DiscordInteractionsService(
                DiscordSocketClient discordClient,
                IDataProvider dataProvider
            )
        {
            _discordClient = discordClient;
            _dataProvider = dataProvider;

            _discordClient.ButtonExecuted += ButtonsClicksHandler;

            DiscordBlackjackHitButton hitButton = new DiscordBlackjackHitButton();
            hitButton.DataProvider = _dataProvider;
            hitButton.DiscordClient = _discordClient;

            DiscordBlackjackStandButton standButton = new DiscordBlackjackStandButton();
            standButton.DataProvider = _dataProvider;
            standButton.DiscordClient = _discordClient;

            buttons.Add(hitButton);
            buttons.Add(standButton);
        }

        #endregion

        #region interaction handlers

        private async Task ButtonsClicksHandler(SocketMessageComponent component)
        {
            foreach (DiscordBlackjackButton button in buttons)
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
