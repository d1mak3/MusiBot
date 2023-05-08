using Discord;
using Discord.WebSocket;
using MusiBotProd.Utilities.Data;
using MusiBotProd.Utilities.Data.DatabaseControllers;
using MusiBotProd.Utilities.Data.Models;

namespace MusiBotProd.Utilities.Visuals.DiscordButtons
{
    /// <summary>
    /// Blackjack button definition
    /// </summary>
    public abstract class DiscordBlackjackButton : IButton
    {
        #region utilities

        public SocketMessageComponent? MessageComponent { set; protected get; }
        public IDataProvider? DataProvider { set; protected get; }
        public IDiscordClient? DiscordClient { set; protected get; }

        #endregion

        #region data fields

        public string Label { protected set; get; }
        public string CustomId { protected set; get; }
        public ButtonStyle Style { protected set; get; }        
        public IEmote Emote { protected set; get; }
        public string Url { protected set; get; }
        public bool IsDisabled { protected set; get; }
        public int Row { protected set; get; }

        #endregion

        #region constructors

        public DiscordBlackjackButton(string label = "", string customId = "",
            ButtonStyle style = ButtonStyle.Success, IEmote emote = null,
            string url = "" , bool isDisabled = false, int row = 0)
        {
            this.Label = label;
            this.CustomId = customId;
            this.Style = style;
            this.Emote = emote;
            this.Url = url;
            this.IsDisabled = isDisabled;
            this.Row = row;            
        }

        #endregion

        #region execution method

        virtual public Task ExecuteAsync() => Task.CompletedTask;

        #endregion

        #region supporting methods

        public void GiveCoinsToUser(string userName, string guildName, int coinsToGive)
        {
            if (DataProvider == null || DiscordClient == null)
            {
                throw new Exception("Incorrect button data");
            }

            SocketGuild guild = (DiscordClient as DiscordSocketClient).Guilds
                .Where(guild => guild.Name == guildName)
                .First();

            ulong guildId = guild.Id;

            ulong userId = guild.Users
                .Where(user => user.Username == userName)
                .First().Id;

            int idOfUserAndGuildConnection = DatabaseController.Contexts.UnGConnections
                .Where(connection =>
                    connection.UserId == $"{userId}" &&
                    connection.GuildId == $"{guildId}")
                .First().Id;

            Balance balance = DatabaseController.Contexts.Balances
                .Where(balance => balance.IdOfUnGConnection == idOfUserAndGuildConnection)
                .First();

            balance.AmountOfCoinsInCash += coinsToGive;

            DataProvider.OpenConnection();

            new DatabaseController(DataProvider).SaveContexts();

            DataProvider.CloseConnection();
        }

        #endregion        
    }
}
