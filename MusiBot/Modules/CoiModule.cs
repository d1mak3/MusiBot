using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MusiBotProd.Utilities.Data;
using MusiBotProd.Utilities.Data.Models;
using MusiBotProd.Utilities.Visuals;
using MusiBotProd.Utilities.Visuals.Buttons;

namespace Example.Modules
{
    [Name("Coi")]
    [RequireContext(ContextType.Guild)]
    public class CoiModule : ModuleBase<SocketCommandContext>
    {
        #region readonlies        

        private readonly IDataProvider _dataProvider;
        private readonly DatabaseController databaseController;

        #endregion

        #region constants

        private const double TIMESPAN_FOR_EARN = 15.0;
        private const double TIMESPAN_FOR_INCOME = 12 * 60;
        private const int MIN_EARN = 50;
        private const int MAX_EARN = 100;

        #endregion

        #region constructors

        public CoiModule(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            databaseController = new DatabaseController(_dataProvider);
        }

        #endregion

        #region commands

        [Command("earn")]
        [Alias("e")]
        [Summary("Earn some coins!")]
        public async Task EarnCoins()
        {
            _dataProvider.OpenConnection();

            SocketUser guildUser = Context.User;

            AddUserIfNotAdded($"{guildUser.Id}", $"{Context.Guild.Id}");

            int idOfUserAndGuildConnection = DatabaseController.Contexts.UnGConnections
                .Where(connection =>
                    connection.UserId == $"{guildUser.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}")
                .First().Id;

            double timePassedFromLastEarning = (DateTime.Now - DatabaseController.Contexts.UnGConnections
                .Where(connection => connection.Id == idOfUserAndGuildConnection)
                .First()
                .EarnTime).TotalMinutes;

            if (timePassedFromLastEarning < TIMESPAN_FOR_EARN)
            {
                await ReplyAsync($"Wait {(int) TIMESPAN_FOR_EARN - (int) timePassedFromLastEarning} minutes!");
                _dataProvider.CloseConnection();
                return;
            }

            int earnedCoins = Random.Shared.Next(MIN_EARN, MAX_EARN);

            DatabaseController.Contexts.Balances
                .Where(balance => balance.IdOfUnGConnection == idOfUserAndGuildConnection)
                .First()
                .AmountOfCoinsInCash += earnedCoins;

            DatabaseController.Contexts.UnGConnections
                .Where(connection => connection.Id == idOfUserAndGuildConnection)
                .First()
                .EarnTime = DateTime.Now;

            databaseController.SaveContexts();

            await ReplyAsync($"You've earned {earnedCoins} coins!");

            _dataProvider.CloseConnection();
        }

        [Command("balance")]
        [Alias("bal")]
        [Summary("Check your balance!")]
        public async Task CheckBalance()
        {
            if (!DatabaseController.Contexts.UnGConnections.Any(
                connection =>
                    connection.UserId == $"{Context.User.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}"
                ))
            {
                await ReplyAsync("Error: You haven't earned coins on this server yet!");
                _dataProvider.CloseConnection();
                return;
            }

            int idOfUserAndGuildConnection = DatabaseController.Contexts.UnGConnections
                .Where(connection =>
                    connection.UserId == $"{Context.User.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}")
                .First().Id;            
            
            int earnedCoins = Random.Shared.Next(MIN_EARN, MAX_EARN);            

            Balance userBalance = DatabaseController.Contexts.Balances
                .Where(balance => balance.IdOfUnGConnection == idOfUserAndGuildConnection)
                .First();

            int cash = userBalance.AmountOfCoinsInCash;

            int dep = userBalance.AmountOfCoinsOnDeposit;

            await ReplyAsync($"Cash: {cash} coin(s)\nOn deposit: {dep} coins(s)");

            _dataProvider.CloseConnection();
        }

        [Command("add_role_income")]
        [Alias("ari")]
        [Summary("Add an income value for a role!")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddIncome(string roleName, int income)
        {
            _dataProvider.OpenConnection();

            if (!Context.Guild.Roles.Any(role => role.Name == roleName))
            {
                await ReplyAsync("No such role!");
                _dataProvider.CloseConnection();
                return;
            }

            if (DatabaseController.Contexts.RnGConnections.Any(connection => connection.RoleName == roleName))
            {
                DatabaseController.Contexts.RnGConnections
                    .Where(connection => connection.RoleName == roleName)
                    .First().Income = income;

                await ReplyAsync("Successfully added!");
                _dataProvider.CloseConnection();
                return;
            }            

            DatabaseController.Contexts.RnGConnections.Add(new RoleAndGuildConnection
            {
                Id = DatabaseController.Contexts.RnGConnections.Count,
                GuildId = $"{Context.Guild.Id}",
                RoleName = roleName,
                Income = income
            });

            Console.WriteLine($"{DatabaseController.Contexts.RnGConnections.Count} -- {DatabaseController.Contexts.RnGConnections[DatabaseController.Contexts.RnGConnections.Count - 1].Id}");

            databaseController.SaveContexts();

            await ReplyAsync("Successfully added!");

            _dataProvider.CloseConnection();
        }

        [Command("check_roles_income")]
        [Alias("cri")]
        [Summary("Check an income value for roles on this server!")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task CheckIncomes()
        {
            _dataProvider.OpenConnection();

            if (!DatabaseController.Contexts.RnGConnections
                .Any(connection => connection.GuildId == $"{Context.Guild.Id}"))
            {
                await ReplyAsync("No roles with income on this server!");
                _dataProvider.CloseConnection();
                return;
            }

            string replyMessage = "";

            foreach(RoleAndGuildConnection connection in DatabaseController.Contexts.RnGConnections
                .Where(connection => connection.GuildId == $"{Context.Guild.Id}"))
            {
                replyMessage += $"{connection.RoleName} - {connection.Income} coin(s)\n";
            }

            await ReplyAsync(replyMessage);

            _dataProvider.CloseConnection();
        }

        [Command("role_income")]
        [Alias("collect", "ri")]
        [Summary("Get an income for the roles you have on this server!")]        
        public async Task GetIncomes()
        {
            _dataProvider.OpenConnection();

            if (!DatabaseController.Contexts.RnGConnections
                .Any(connection => connection.GuildId == $"{Context.Guild.Id}"))
            {
                await ReplyAsync("No roles with income on this server!");
                _dataProvider.CloseConnection();
                return;
            }

            int idOfUserAndGuildConnection = DatabaseController.Contexts.UnGConnections
                .Where(connection =>
                    connection.UserId == $"{Context.User.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}")
                .First().Id;

            double timePassedFromLastEarning = (DateTime.Now - DatabaseController.Contexts.UnGConnections
                .Where(connection => connection.Id == idOfUserAndGuildConnection)
                .First()
                .RoleIncomeTime).TotalMinutes;

            if (timePassedFromLastEarning < TIMESPAN_FOR_INCOME)
            {
                await ReplyAsync($"Wait {((int)TIMESPAN_FOR_INCOME - (int)timePassedFromLastEarning) / 60} hours" +
                    $" {((int)TIMESPAN_FOR_INCOME - (int)timePassedFromLastEarning) % 60} minutes!");
                _dataProvider.CloseConnection();
                return;
            }

            string replyMessage = "";

            foreach (RoleAndGuildConnection connection in DatabaseController.Contexts.RnGConnections
                .Where(connection => connection.GuildId == $"{Context.Guild.Id}"))
            {
                replyMessage += $"You've collected income for {connection.RoleName} - {connection.Income} coin(s)\n";
                DatabaseController.Contexts.Balances
                    .Where(balance => balance.IdOfUnGConnection == idOfUserAndGuildConnection)
                    .First().AmountOfCoinsInCash += connection.Income;
            }

            DatabaseController.Contexts.UnGConnections
                .Where(connection => connection.Id == idOfUserAndGuildConnection)
                .First()
                .RoleIncomeTime = DateTime.Now;

            databaseController.SaveContexts();

            await ReplyAsync(replyMessage);

            _dataProvider.CloseConnection();
        }

        [Command("on_deposit")]
        [Alias("dep", "od")]
        [Summary("Put your coins on deposit!")]
        public async Task PutOnDeposit(int value)
        {
            _dataProvider.OpenConnection();

            int idOfUnGConnection = DatabaseController.Contexts.UnGConnections
                .Where(connection =>
                    connection.UserId == $"{Context.User.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}"
                ).First().Id;

            if (!DatabaseController.Contexts.Balances.Any(connection => 
                    connection.IdOfUnGConnection == idOfUnGConnection
                ))
            {
                await ReplyAsync("Error: You haven't earned coins on this server yet!");
                _dataProvider.CloseConnection();
                return;
            }

            Balance balance = DatabaseController.Contexts.Balances
                .Where(connection =>
                    connection.IdOfUnGConnection == idOfUnGConnection)
                .First();

            if (balance.AmountOfCoinsInCash == 0)
            {
                await ReplyAsync("Error: You have no coins in cash!");
                _dataProvider.CloseConnection();
                return;
            }

            if (balance.AmountOfCoinsInCash < value)
            {
                await ReplyAsync("Error: You don't have enough coins in cash!");
                _dataProvider.CloseConnection();
                return;
            }

            balance.AmountOfCoinsOnDeposit += value;
            balance.AmountOfCoinsInCash -= value;

            await ReplyAsync("You've put your coins on deposit!");

            databaseController.SaveContexts();

            _dataProvider.CloseConnection();
        }

        [Command("on_deposit")]
        [Alias("dep", "od")]
        [Summary("Put your coins on deposit!")]
        public async Task PutOnDeposit(string value)
        {
            _dataProvider.OpenConnection();

            int idOfUnGConnection = DatabaseController.Contexts.UnGConnections
                .Where(connection =>
                    connection.UserId == $"{Context.User.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}")
                .First().Id;

            if (!DatabaseController.Contexts.Balances.Any(connection =>
                    connection.IdOfUnGConnection == idOfUnGConnection
                ))
            {
                await ReplyAsync("Error: You haven't earned coins on this server yet!");
                _dataProvider.CloseConnection();
                return;
            }

            if (value.ToLower() != "all")
            {
                await ReplyAsync("Error: You have to enter a number or \"all\"");
                _dataProvider.CloseConnection();
                return;
            }

            Balance balance = DatabaseController.Contexts.Balances
                .Where(connection =>
                    connection.IdOfUnGConnection == idOfUnGConnection)
                .First();

            if (balance.AmountOfCoinsInCash == 0)
            {
                await ReplyAsync("Error: You have no coins in cash!");
                _dataProvider.CloseConnection();
                return;
            }

            balance.AmountOfCoinsOnDeposit += balance.AmountOfCoinsInCash;
            balance.AmountOfCoinsInCash = 0;

            await ReplyAsync("You've put your coins on deposit!");

            databaseController.SaveContexts();

            _dataProvider.CloseConnection();
        }

        [Command("in_cash")]
        [Alias("with", "cash", "ic")]
        [Summary("Take your coins from deposit!")]
        public async Task TakeFromDeposit(int value)
        {
            _dataProvider.OpenConnection();

            int idOfUnGConnection = DatabaseController.Contexts.UnGConnections
                .Where(connection =>
                   connection.UserId == $"{Context.User.Id}" &&
                   connection.GuildId == $"{Context.Guild.Id}")
                .First().Id;

            if (!DatabaseController.Contexts.Balances.Any(connection =>
                    connection.IdOfUnGConnection == idOfUnGConnection
                ))
            {
                await ReplyAsync("Error: You haven't earned coins on this server yet!");
                _dataProvider.CloseConnection();
                return;
            }

            Balance balance = DatabaseController.Contexts.Balances
                .Where(connection =>
                    connection.IdOfUnGConnection == idOfUnGConnection)
                .First();

            if (balance.AmountOfCoinsOnDeposit == 0)
            {
                await ReplyAsync("Error: You have no coins on deposit!");
                _dataProvider.CloseConnection();
                return;
            }

            if (balance.AmountOfCoinsOnDeposit < value)
            {
                await ReplyAsync("Error: You don't have enough coins on deposit!");
                _dataProvider.CloseConnection();
                return;
            }

            balance.AmountOfCoinsInCash += value;
            balance.AmountOfCoinsOnDeposit -= value;

            await ReplyAsync("You've taken your coins from deposit!");

            databaseController.SaveContexts();

            _dataProvider.CloseConnection();
        }

        [Command("in_cash")]
        [Alias("with", "cash", "ic")]
        [Summary("Take your coins from deposit!")]
        public async Task TakeFromDeposit(string value)
        {
            _dataProvider.OpenConnection();

            if (value.ToLower() != "all")
            {
                await ReplyAsync("Error: You have to enter a number or \"all\"");
                return;
            }

            int idOfUnGConnection = DatabaseController.Contexts.UnGConnections
                .Where(connection =>
                   connection.UserId == $"{Context.User.Id}" &&
                   connection.GuildId == $"{Context.Guild.Id}")
                .First().Id;

            if (!DatabaseController.Contexts.Balances.Any(connection =>
                    connection.IdOfUnGConnection == idOfUnGConnection
                ))
            {
                await ReplyAsync("Error: You haven't earned coins on this server yet!");
                _dataProvider.CloseConnection();
                return;
            }

            Balance balance = DatabaseController.Contexts.Balances
                .Where(connection =>
                    connection.IdOfUnGConnection == idOfUnGConnection)
                .First();

            if (balance.AmountOfCoinsOnDeposit == 0)
            {
                await ReplyAsync("Error: You have no coins on deposit!");
                _dataProvider.CloseConnection();
                return;
            }

            balance.AmountOfCoinsInCash += balance.AmountOfCoinsOnDeposit;
            balance.AmountOfCoinsOnDeposit = 0;

            await ReplyAsync("You've taken your coins from deposit!");

            databaseController.SaveContexts();

            _dataProvider.CloseConnection();
        }

        [Command("fuck_your_coins")]
        [Alias("fyc", "delete_coins", "delc")]
        [Summary("Someone's coins are vanished lmao!")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task VanishCoins(SocketGuildUser user)
        {
            _dataProvider.OpenConnection();

            if (user == null)
            {
                await ReplyAsync("Error: No such user!");
                _dataProvider.CloseConnection();
                return;
            }

            if (!DatabaseController.Contexts.UnGConnections.Any(connection => 
                    connection.UserId == $"{user.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}"
                ))
            {
                await ReplyAsync("Error: User hasn't earned coins on this server yet!");
                _dataProvider.CloseConnection();
                return;
            }

            int idOfUnGConnection = DatabaseController.Contexts.UnGConnections
                .Where(connection =>
                    connection.UserId == $"{user.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}")
                .First().Id;

            Balance balance = DatabaseController.Contexts.Balances
                                .Where(balance => balance.IdOfUnGConnection == idOfUnGConnection)
                                .First();

            balance.AmountOfCoinsOnDeposit = 0;
            balance.AmountOfCoinsInCash = 0;

            databaseController.SaveContexts();

            await ReplyAsync($"{user.Nickname}'s coins have been vanished!");

            _dataProvider.CloseConnection();
        }

        [Command("add_coins")]
        [Alias("ac", "addc")]
        [Summary("Add coins to someone!")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddCoins(SocketGuildUser user, int value)
        {
            _dataProvider.OpenConnection();

            if (user == null)
            {
                await ReplyAsync("Error: No such user!");
                _dataProvider.CloseConnection();
                return;
            }

            AddUserIfNotAdded($"{user.Id}", $"{Context.Guild.Id}");

            int idOfUnGConnection = DatabaseController.Contexts.UnGConnections
                .Where(connection =>
                    connection.UserId == $"{user.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}")
                .First().Id;

            Balance balance = DatabaseController.Contexts.Balances
                                .Where(balance => balance.IdOfUnGConnection == idOfUnGConnection)
                                .First();

            balance.AmountOfCoinsInCash += value;

            await ReplyAsync($"Coins added to {user.Nickname}: {value}");            
            databaseController.SaveContexts();

            _dataProvider.CloseConnection();
        }

        [Command("rob")]
        [Alias()]
        [Summary("Rob someone!")]
        public async Task Rob(SocketGuildUser user)
        {
            _dataProvider.OpenConnection();

            if (user == null)
            {
                await ReplyAsync("Error: No such user!");
                _dataProvider.CloseConnection();
                return;
            }

            if (!DatabaseController.Contexts.UnGConnections
                .Any(connection =>
                    connection.UserId == $"{user.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}"))
            {
                await ReplyAsync("Error: User hasn't earned coins on this server yet!");
                _dataProvider.CloseConnection();
                return;
            }

            int idOfUnGConnectionToRob = DatabaseController.Contexts.UnGConnections
                .Where(connection =>
                    connection.UserId == $"{user.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}")
                .First().Id;
            
            int idOfUnGConnectionToAdd = DatabaseController.Contexts.UnGConnections
                .Where(connection =>
                    connection.UserId == $"{Context.User.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}")
                .First().Id;

            Balance balanceToRob = DatabaseController.Contexts.Balances
                .Where(balance => balance.IdOfUnGConnection == idOfUnGConnectionToRob)
                .First();
            
            Balance balanceToAdd = DatabaseController.Contexts.Balances
                .Where(balance => balance.IdOfUnGConnection == idOfUnGConnectionToAdd)
                .First();

            if (balanceToRob.AmountOfCoinsInCash == 0)
            {
                await ReplyAsync("Error: User doesn't have coins in cash!");
                _dataProvider.CloseConnection();
                return;
            }

            int amountOfLootedCoins = Random.Shared.Next(0, balanceToRob.AmountOfCoinsInCash);

            balanceToRob.AmountOfCoinsInCash -= amountOfLootedCoins;
            balanceToAdd.AmountOfCoinsInCash += amountOfLootedCoins;

            databaseController.SaveContexts();

            await ReplyAsync($"You successfully robbed {amountOfLootedCoins} coin(s) from {user.Nickname}!");
            
            _dataProvider.CloseConnection();
        }

        [Command("roulette")]
        [Alias("rlte", "rt")]
        [Summary("Play roulette!")]
        public async Task PlayRoulette(int betNumber, int betCoins)
        {
            _dataProvider.OpenConnection();

            if (!DatabaseController.Contexts.UnGConnections.Any(
                connection =>
                    connection.UserId == $"{Context.User.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}"
                ))
            {
                await ReplyAsync("Error: You haven't earned coins on this server yet!");
                _dataProvider.CloseConnection();
                return;
            }

            if (betCoins < 1)
            {
                await ReplyAsync("Error: Minimum bet is 1!");
                _dataProvider.CloseConnection();
                return;
            }

            int idOfUnGConnection = DatabaseController.Contexts.UnGConnections
                .Where(connection =>
                    connection.UserId == $"{Context.User.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}")
                .First().Id;

            Balance balance = DatabaseController.Contexts.Balances
                .Where(balance => balance.IdOfUnGConnection == idOfUnGConnection)
                .First();
            
            if (balance.AmountOfCoinsInCash < betCoins)
            {
                await ReplyAsync("Error: You don't have enough coins in cash!");
                _dataProvider.CloseConnection();
                return;
            }

            if (betNumber > 36 && betNumber < 0)
            {
                await ReplyAsync("Error: You can only bet on red, black or a positive number less than 37!");
                _dataProvider.CloseConnection();
                return;
            }

            balance.AmountOfCoinsInCash -= betCoins;

            int rouletteResult = Random.Shared.Next(0, 36);
            
            if (rouletteResult == betNumber && rouletteResult == 0)
            {
                balance.AmountOfCoinsInCash += betCoins * 36;

                databaseController.SaveContexts();

                await ReplyAsync($"Congratulations! You've won {betCoins * 36}");
                _dataProvider.CloseConnection();
                return;
            }

            if (rouletteResult == betNumber)
            {
                balance.AmountOfCoinsInCash += betCoins * 18;

                databaseController.SaveContexts();

                await ReplyAsync($"Congratulations! You've won {betCoins * 18}");
                _dataProvider.CloseConnection();
                return;
            }

            databaseController.SaveContexts();

            await ReplyAsync($"You've lost! The ball landed on {rouletteResult}!");

            _dataProvider.CloseConnection();
        }

        [Command("roulette")]
        [Alias("rlte", "rt")]
        [Summary("Play roulette!")]
        public async Task PlayRoulette(int betNumber, string value)
        {
            _dataProvider.OpenConnection();

            if (!DatabaseController.Contexts.UnGConnections.Any(
                connection =>
                    connection.UserId == $"{Context.User.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}"
                ))
            {
                await ReplyAsync("Error: You haven't earned coins on this server yet!");
                _dataProvider.CloseConnection();
                return;
            }

            if (value.ToLower() != "all")
            {
                await ReplyAsync("Error: You have to enter a number or \"all\"");
                _dataProvider.CloseConnection();
                return;
            }

            int idOfUnGConnection = DatabaseController.Contexts.UnGConnections
                .Where(connection =>
                    connection.UserId == $"{Context.User.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}")
                .First().Id;

            Balance balance = DatabaseController.Contexts.Balances
                .Where(balance => balance.IdOfUnGConnection == idOfUnGConnection)
                .First();

            if (balance.AmountOfCoinsInCash == 0)
            {
                await ReplyAsync("Error: You don't have coins in cash!");
                _dataProvider.CloseConnection();
                return;
            }

            if (betNumber > 36 && betNumber < 0)
            {
                await ReplyAsync("Error: You can only bet on red, black or a positive number less than 37!");
                _dataProvider.CloseConnection();
                return;
            }

            int betCoins = balance.AmountOfCoinsInCash;
            balance.AmountOfCoinsInCash = 0;

            int rouletteResult = Random.Shared.Next(0, 36);

            if (rouletteResult == betNumber && rouletteResult == 0)
            {
                balance.AmountOfCoinsInCash += betCoins * 36;

                databaseController.SaveContexts();

                await ReplyAsync($"Congratulations! You've won {betCoins * 36}");
                _dataProvider.CloseConnection();
                return;
            }

            if (rouletteResult == betNumber)
            {
                balance.AmountOfCoinsInCash += betCoins * 18;

                databaseController.SaveContexts();

                await ReplyAsync($"Congratulations! You've won {betCoins * 18}");
                _dataProvider.CloseConnection();
                return;
            }

            databaseController.SaveContexts();

            await ReplyAsync($"You lost! The ball landed on {rouletteResult}");

            _dataProvider.CloseConnection();
        }

        [Command("roulette")]
        [Alias("rlte", "rt")]
        [Summary("Play roulette!")]
        public async Task PlayRoulette(string betColour, int betCoins)
        {
            _dataProvider.OpenConnection();

            if (!DatabaseController.Contexts.UnGConnections.Any(
                connection =>
                    connection.UserId == $"{Context.User.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}"
                ))
            {
                await ReplyAsync("Error: You haven't earned coins on this server yet!");
                _dataProvider.CloseConnection();
                return;
            }

            if (betCoins < 1)
            {
                await ReplyAsync("Error: Minimum bet is 1!");
                _dataProvider.CloseConnection();
                return;
            }

            int idOfUnGConnection = DatabaseController.Contexts.UnGConnections
                .Where(connection =>
                    connection.UserId == $"{Context.User.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}")
                .First().Id;

            Balance balance = DatabaseController.Contexts.Balances
                .Where(balance => balance.IdOfUnGConnection == idOfUnGConnection)
                .First();

            if (balance.AmountOfCoinsInCash < betCoins)
            {
                await ReplyAsync("Error: You don't have enough coins in cash!");
                _dataProvider.CloseConnection();
                return;
            }

            if (betColour.ToLower() != "red" && betColour.ToLower() != "black")
            {
                await ReplyAsync("Error: You can only bet on red, black or a positive number less than 37!");
                _dataProvider.CloseConnection();
                return;
            }

            balance.AmountOfCoinsInCash -= betCoins;

            int rouletteResult = Random.Shared.Next(0, 36);
            string resultColour = IsRouletteNumberRed(rouletteResult) ? "red" : "black";

            if (resultColour == betColour)
            {
                balance.AmountOfCoinsInCash += betCoins * 2;

                databaseController.SaveContexts();

                await ReplyAsync($"Congratulations! You've won {betCoins * 2}");
                _dataProvider.CloseConnection();
                return;
            }

            databaseController.SaveContexts();

            await ReplyAsync($"You've lost! The ball landed on {rouletteResult} {resultColour}!");
            
            _dataProvider.CloseConnection();
        }

        [Command("roulette")]
        [Alias("rlte", "rt")]
        [Summary("Play roulette!")]
        public async Task PlayRoulette(string betColour, string value)
        {
            _dataProvider.OpenConnection();

            if (!DatabaseController.Contexts.UnGConnections.Any(
                connection =>
                    connection.UserId == $"{Context.User.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}"
                ))
            {
                await ReplyAsync("Error: You haven't earned coins on this server yet!");
                _dataProvider.CloseConnection();
                return;
            }

            if (value.ToLower() != "all")
            {
                await ReplyAsync("Error: You have to enter a number or \"all\"");
                _dataProvider.CloseConnection();
                return;
            }

            int idOfUnGConnection = DatabaseController.Contexts.UnGConnections
                .Where(connection =>
                    connection.UserId == $"{Context.User.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}")
                .First().Id;

            Balance balance = DatabaseController.Contexts.Balances
                .Where(balance => balance.IdOfUnGConnection == idOfUnGConnection)
                .First();

            if (balance.AmountOfCoinsInCash == 0)
            {
                await ReplyAsync("Error: You don't have enough coins in cash!");
                _dataProvider.CloseConnection();
                return;
            }

            if (betColour.ToLower() != "red" && betColour.ToLower() != "black")
            {
                await ReplyAsync("Error: You can only bet on red, black or a positive number less than 37!");
                _dataProvider.CloseConnection();
                return;
            }

            int betCoins = balance.AmountOfCoinsInCash;
            balance.AmountOfCoinsInCash = 0;

            int rouletteResult = Random.Shared.Next(0, 36);
            string resultColour = IsRouletteNumberRed(rouletteResult) ? "red" : "black";

            if (resultColour == betColour)
            {
                balance.AmountOfCoinsInCash = betCoins * 2;

                databaseController.SaveContexts();

                await ReplyAsync($"Congratulations! You've won {betCoins * 2}");
                _dataProvider.CloseConnection();
                return;
            }

            databaseController.SaveContexts();

            await ReplyAsync($"You've lost! The ball landed on {rouletteResult} {resultColour}!");
            
            _dataProvider.CloseConnection();
        }

        [Command("leaderboard")]
        [Alias("lb")]
        [Summary("Check this server's leaderboard!")]
        public async Task CheckLeaderboard()
        {
            _dataProvider.OpenConnection();

            if (!DatabaseController.Contexts.UnGConnections.Any(connection =>
                    connection.GuildId == $"{Context.Guild.Id}"
                ))
            {
                await ReplyAsync("Error: There is no players who've earned coins on this server!");
                _dataProvider.CloseConnection();
                return;
            }

            string leaderboardString = $"**{Context.Guild.Name}**'s leaderboard:\n";

            IEnumerable<UserAndGuildConnection> UnGConnections = DatabaseController.Contexts.UnGConnections
                .Where(connection => connection.GuildId == $"{Context.Guild.Id}");
            List<Balance> balances = new List<Balance>();

            foreach (UserAndGuildConnection connection in UnGConnections)
            {
                balances.Add(DatabaseController.Contexts.Balances
                        .Where(balance => balance.IdOfUnGConnection == connection.Id)
                        .First());
            }

            IOrderedEnumerable<Balance> orderedBalances = balances
                .OrderByDescending(balance => balance.AmountOfCoinsInCash + balance.AmountOfCoinsOnDeposit);

            for (int i = 0; i < 10; i++)
            {
                if (i >= orderedBalances.Count())
                {
                    break;
                }

                Balance currentBalance = orderedBalances.ElementAt(i);

                int idOfUnGConnection = currentBalance.IdOfUnGConnection;
                string userId = DatabaseController.Contexts.UnGConnections
                    .Where(connection => connection.Id == idOfUnGConnection)
                    .First().UserId;
                string userNickname = Context.Guild.Users
                    .Where(user => $"{user.Id}" == userId)
                    .First().Nickname;

                leaderboardString +=
                    $"{i + 1}. {userNickname} —" +
                    $" {currentBalance.AmountOfCoinsInCash + currentBalance.AmountOfCoinsOnDeposit}\n";
            }

            await ReplyAsync(leaderboardString);

            _dataProvider.CloseConnection();
        }

        [Command("leaderboard")]
        [Alias("lb")]
        [Summary("Check this server's leaderboard!")]
        public async Task CheckLeaderboard(string option)
        {
            _dataProvider.OpenConnection();

            if (!DatabaseController.Contexts.UnGConnections.Any(connection =>
                    connection.GuildId == $"{Context.Guild.Id}"
                ))
            {
                await ReplyAsync("Error: There is no players who've earned coins on this server!");
                _dataProvider.CloseConnection();
                return;
            }

            if (option != "cash" && option != "dep" && option != "deposit")
            {
                await ReplyAsync("Error: You have to enter \"cash\", \"dep\" or \"deposit\""!);
                _dataProvider.CloseConnection();
                return;
            }

            string leaderboardString = $"**{Context.Guild.Name}**'s leaderboard:\n";

            IEnumerable<UserAndGuildConnection> UnGConnections = DatabaseController.Contexts.UnGConnections
                .Where(connection => connection.GuildId == $"{Context.Guild.Id}");
            List<Balance> balances = new List<Balance>();

            foreach (UserAndGuildConnection connection in UnGConnections)
            {
                balances.Add(DatabaseController.Contexts.Balances
                    .Where(balance => balance.IdOfUnGConnection == connection.Id)
                    .First());
            }

            IOrderedEnumerable<Balance> orderedBalances;

            if (option == "cash")
            {
                orderedBalances = balances.OrderByDescending(balance => balance.AmountOfCoinsInCash);
            }
            else
            {
                orderedBalances = balances
                .OrderByDescending(balance => balance.AmountOfCoinsOnDeposit);
            }

            for (int i = 0; i < 10; i++)
            {
                if (i >= orderedBalances.Count())
                {
                    break;
                }

                Balance currentBalance = orderedBalances.ElementAt(i);

                int idOfUnGConnection = currentBalance.IdOfUnGConnection;
                string userId = DatabaseController.Contexts.UnGConnections.Where(connection =>
                    connection.Id == idOfUnGConnection).First().UserId;
                string userNickname = Context.Guild.Users
                    .Where(user => $"{user.Id}" == userId)
                    .First()
                    .Nickname;

                if (option == "cash")
                {
                    leaderboardString +=
                    $"{i + 1}. {userNickname} —" +
                    $" {currentBalance.AmountOfCoinsInCash}\n";
                }
                else
                {
                    leaderboardString +=
                    $"{i + 1}. {userNickname} —" +
                    $" {currentBalance.AmountOfCoinsOnDeposit}\n";
                }
            }

            await ReplyAsync(leaderboardString);

            _dataProvider.CloseConnection();
        }

        [Command("blackjack")]
        [Alias("bj")]
        [Summary("Play blackjack!")]
        public async Task PlayBlackjack(int betCoins)
        {
            _dataProvider.OpenConnection();

            if (!DatabaseController.Contexts.UnGConnections.Any(
                connection =>
                    connection.UserId == $"{Context.User.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}"
                ))
            {
                await ReplyAsync("Error: You haven't earned coins on this server yet!");
                _dataProvider.CloseConnection();
                return;
            }

            if (betCoins < 1)
            {
                await ReplyAsync("Error: Minimum bet is 1!");
                _dataProvider.CloseConnection();
                return;
            }

            int idOfUnGConnection = DatabaseController.Contexts.UnGConnections
                .Where(connection =>
                    connection.UserId == $"{Context.User.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}"
                ).First().Id;

            Balance balance = DatabaseController.Contexts.Balances
                .Where(balance =>
                    balance.IdOfUnGConnection == idOfUnGConnection
                ).First();

            if (balance.AmountOfCoinsInCash < betCoins)
            {
                await ReplyAsync("Error: You don't have enough coins in cash!");
                _dataProvider.CloseConnection();
                return;
            }

            balance.AmountOfCoinsInCash -= betCoins;

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

            string blackjackMessage = $"{Context.User.Username} hand: {usersHandValue}\n" +
                $"Dealers hand: {dealersHandValue}\nBet is: {betCoins}\nGuild: {Context.Guild.Name}";

            BlackjackHitButton hitButton = new BlackjackHitButton();
            BlackjackStandButton standButton = new BlackjackStandButton();

            ComponentBuilder builder = new ComponentBuilder();

            AddButtonToComponent(ref builder, hitButton);
            AddButtonToComponent(ref builder, standButton);

            await ReplyAsync(blackjackMessage, components: builder.Build());

            _dataProvider.CloseConnection();
        }

        [Command("blackjack")]
        [Alias("bj")]
        [Summary("Play blackjack!")]
        public async Task PlayBlackjack(string value)
        {
            _dataProvider.OpenConnection();

            if (!DatabaseController.Contexts.UnGConnections.Any(
                connection =>
                    connection.UserId == $"{Context.User.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}"
                ))
            {
                await ReplyAsync("Error: You haven't earned coins on this server yet!");
                return;
            }

            if (value.ToLower() != "all")
            {
                await ReplyAsync("Error: You have to enter a number or \"all\"");
                return;
            }

            int idOfUnGConnection = DatabaseController.Contexts.UnGConnections
                .Where(connection =>
                    connection.UserId == $"{Context.User.Id}" &&
                    connection.GuildId == $"{Context.Guild.Id}"
                ).First().Id;

            Balance balance = DatabaseController.Contexts.Balances
                .Where(balance =>
                    balance.IdOfUnGConnection == idOfUnGConnection
                ).First();

            if (balance.AmountOfCoinsInCash == 0)
            {
                await ReplyAsync("Error: You don't have enough coins in cash!");
                _dataProvider.CloseConnection();
                return;
            }

            int betCoins = balance.AmountOfCoinsInCash;
            balance.AmountOfCoinsInCash = 0;

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

            string blackjackMessage = $"{Context.User.Username} hand: {usersHandValue}\n" +
                $"Dealers hand: {dealersHandValue}\nBet is: {betCoins}\nGuild: {Context.Guild.Name}";

            BlackjackHitButton hitButton = new BlackjackHitButton();
            BlackjackStandButton standButton = new BlackjackStandButton();

            ComponentBuilder builder = new ComponentBuilder();

            AddButtonToComponent(ref builder, hitButton);
            AddButtonToComponent(ref builder, standButton);

            await ReplyAsync(blackjackMessage, components: builder.Build());

            _dataProvider.CloseConnection();
        }

        #endregion

        #region supporting methods

        private void AddUserIfNotAdded(string userId, string guildId)
        {
            if (!DatabaseController.Contexts.Users.Any(user => user.Id == userId))
            {                
                DatabaseController.Contexts.Users.Add(new User { Id = userId });
            }

            if (!DatabaseController.Contexts.Guilds.Any(guild => guild.Id == guildId))
            {
                DatabaseController.Contexts.Guilds.Add(new Guild { Id = guildId });
            }

            if (!DatabaseController.Contexts.UnGConnections.Any(connection =>
                connection.UserId == userId &&
                connection.GuildId == guildId))
            {
                DatabaseController.Contexts.UnGConnections.Add(
                    new UserAndGuildConnection
                    {
                        Id = DatabaseController.Contexts.UnGConnections.Count,
                        UserId = userId,
                        GuildId = guildId,
                        EarnTime = DateTime.MinValue,
                        RoleIncomeTime = DateTime.MinValue
                    });
            }

            int idOfUserAndGuildConnection = DatabaseController.Contexts.UnGConnections
                .Where(connection =>
                    connection.UserId == userId &&
                    connection.GuildId == guildId)
                .First().Id;

            if (!DatabaseController.Contexts.Balances.Any(balance =>
                balance.IdOfUnGConnection == idOfUserAndGuildConnection))
            {
                DatabaseController.Contexts.Balances.Add(
                    new Balance
                    {
                        Id = DatabaseController.Contexts.Balances.Count,
                        IdOfUnGConnection = idOfUserAndGuildConnection,
                        AmountOfCoinsOnDeposit = 0,
                        AmountOfCoinsInCash = 0
                    });
            }           
        }

        private bool IsRouletteNumberRed(int number)
        {
            if (0 < number && number < 11)
            {
                return !(number % 2 == 0);               
            }

            if (10 < number && number < 19)
            {
                return number % 2 == 0;
            }

            if (18 < number && number < 29)
            {
                return !(number % 2 == 0);
            }

            if (28 < number)
            {
                return number % 2 == 0;
            }

            return false;
        }

        private void AddButtonToComponent(ref ComponentBuilder component, BlackjackButton button)
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
        
        #endregion
    }
}
