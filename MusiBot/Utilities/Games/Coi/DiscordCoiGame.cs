using Discord;
using Discord.WebSocket;
using MusiBotProd.Utilities.Data;
using MusiBotProd.Utilities.Data.DatabaseControllers;
using MusiBotProd.Utilities.Data.Models;
using MusiBotProd.Utilities.Visuals.DiscordButtons;

namespace MusiBotProd.Utilities.Games.Coi
{
    public class DiscordCoiGame : CoiGame
    {
        //private readonly IDiscordClient _discordClient;

        public DiscordCoiGame(
            IDataProvider dataProvider,
            IDatabaseController databaseController) 
            : base(DatabaseController.Contexts, dataProvider, databaseController)
        {
            //_discordClient = discordClient;
        }

        public override string AddCoins(ulong userId, ulong guildId, int value)
        {
            DiscordSocketClient _discordClient = new DiscordSocketClient();

            if (_discordClient.GetUser(userId) is null)
            {
                return "Error: User not found!";
            }

            return base.AddCoins(userId, guildId, value);
        }

        public override string AddIncome(ulong guildId, string roleName, int income)
        {
            DiscordSocketClient _discordClient = new DiscordSocketClient();
            _dataProvider.OpenConnection();

            if (income < 0)
            {
                return "Error: You have to enter a positive number!";
            }

            SocketGuild guild = _discordClient.GetGuild(guildId);

            if (!guild.Roles.Any(role => role.Name == roleName))
            {                
                _dataProvider.CloseConnection();
                return "No such role!";
            }

            if (DatabaseController.Contexts.RnGConnections.Any(connection => connection.RoleName == roleName))
            {
                DatabaseController.Contexts.RnGConnections
                    .Where(connection => connection.RoleName == roleName)
                    .First().Income = income;
                
                _dataProvider.CloseConnection();
                return "Successfully added!";
            }

            DatabaseController.Contexts.RnGConnections.Add(new RoleAndGuildConnection
            {
                Id = DatabaseController.Contexts.RnGConnections.Count,
                GuildId = $"{guildId}",
                RoleName = roleName,
                Income = income
            });

            Console.WriteLine($"{DatabaseController.Contexts.RnGConnections.Count} -- {DatabaseController.Contexts.RnGConnections[^1].Id}");

            _databaseController.SaveContexts();            
            _dataProvider.CloseConnection();
            return "Successfully added!";
        }

        public override string BuyRole(ulong userId, ulong guildId, string roleName)
        {
            throw new NotImplementedException();
        }

        public override object[] PlayBlackJack(ulong userId, ulong guildId, object betCoins)
        {
            DiscordSocketClient _discordClient = new DiscordSocketClient();
            _dataProvider.OpenConnection();

            UserAndGuildConnection UnGConnection = DatabaseController.Contexts.UnGConnections
                .Where(connection =>
                    connection.UserId == $"{userId}" &&
                    connection.GuildId == $"{guildId}"
                ).First();

            Balance balance = DatabaseController.Contexts.Balances
                .Where(balance => balance.IdOfUnGConnection == UnGConnection.Id)
                .First();

            if (UnGConnection is null)
            {                
                _dataProvider.CloseConnection();
                return new object[] { "Error: You haven't earned coins on this server yet!" };
            }

            if (!int.TryParse(betCoins.ToString(), out int amountOfCoinsToBet) &&
                !betCoins.ToString().Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                _dataProvider.CloseConnection();
                return new object[] { "Error: You have to enter a positive number or \"all\"" };
            }

            if (betCoins.ToString().Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                amountOfCoinsToBet = balance.AmountOfCoinsInCash;
            }

            if (amountOfCoinsToBet < 0)
            {
                return new object[] { "Error: You have to enter a positive number!" };
            }

            if (amountOfCoinsToBet < 1)
            {
                _dataProvider.CloseConnection();
                return new object[] { "Error: Minimum bet is 1!" };
            }

            if (balance.AmountOfCoinsInCash < amountOfCoinsToBet)
            {
                _dataProvider.CloseConnection();
                return new object[] { "Error: You don't have enough coins in cash!" };
            }

            balance.AmountOfCoinsInCash -= amountOfCoinsToBet;

            int usersHandValue = Random.Shared.Next(3, 21);

            int dealersHandValue = Random.Shared.Next(3, 21);

            if (usersHandValue == 3)
            {
                usersHandValue = 13;
            }

            if (dealersHandValue == 3)
            {
                dealersHandValue = 13;
            }

            string blackjackMessage = $"{_discordClient.GetUser(userId).Username} hand: {usersHandValue}\n" +
                $"Dealers hand: {dealersHandValue}\nBet is: {betCoins}\nGuild: {_discordClient.GetGuild(userId).Name}";

            DiscordBlackjackHitButton hitButton = new DiscordBlackjackHitButton();
            DiscordBlackjackStandButton standButton = new DiscordBlackjackStandButton();

            ComponentBuilder builder = new ComponentBuilder();

            AddButtonToComponent(ref builder, hitButton);
            AddButtonToComponent(ref builder, standButton);            

            _dataProvider.CloseConnection();

            return new object[] { blackjackMessage, builder.Build() };
        }

        private static void AddButtonToComponent(ref ComponentBuilder component, DiscordBlackjackButton button)
        {
            component.WithButton(
                label: button.Label,
                customId: button.CustomId,
                style: button.Style,
                emote: button.Emote,
                url: button.Url,
                disabled: button.IsDisabled,
                row: button.Row
            );
        }
    }
}
