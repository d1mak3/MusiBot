using Discord;
using MusiBotProd.Utilities.Data;
using MusiBotProd.Utilities.Data.DatabaseControllers;
using MusiBotProd.Utilities.Data.Models;
using System;

namespace MusiBotProd.Utilities.Games.Coi
{
    public abstract class CoiGame : ICoiGame
    {
        private const double TIMESPAN_FOR_EARN = 60.0 / 4.0;
        private const double TIMESPAN_FOR_INCOME = 60.0 * 12.0;
        private const int MIN_EARN = 50;
        private const int MAX_EARN = 100;
        private const int ROULETTE_MINIMAL_BET = 1;
        private const int ROULETTE_COLOUR_WIN_MULTIPLIER = 2;
        private const int ROULETTE_ZERO_WIN_MULTIPLIER = 36;
        private const int ROULETTE_NUMBER_WIN_MULTIPLIER = 18;
        private const int LEADERBOARD_SIZE = 10;

        protected readonly DataContexts _contexts;
        protected readonly IDataProvider _dataProvider;
        protected readonly IDatabaseController _databaseController;        

        public CoiGame(DataContexts contexts, IDataProvider dataProvider, IDatabaseController databaseController)
        {
            _contexts = contexts;
            _dataProvider = dataProvider;
            _databaseController = databaseController;            
        }

        public string EarnCoins(ulong userId, ulong guildId)
        {
            _dataProvider.OpenConnection();

            AddUserIfNotAdded(userId, guildId);

            UserAndGuildConnection UnGConnection = GetUserAndGuildConnection(userId, guildId);
            Balance balance = GetBalance(UnGConnection.Id);

            double timePassedFromLastEarning = (DateTime.Now - UnGConnection.EarnTime).TotalMinutes;

            if (timePassedFromLastEarning < TIMESPAN_FOR_EARN)
            {
                _dataProvider.CloseConnection();
                return $"Wait {(int)TIMESPAN_FOR_EARN - (int)timePassedFromLastEarning} minutes!";
            }

            int earnedCoins = Random.Shared.Next(MIN_EARN, MAX_EARN);

            balance.AmountOfCoinsInCash += earnedCoins;

            UnGConnection.EarnTime = DateTime.Now;

            _databaseController.SaveContexts();
            _dataProvider.CloseConnection();

            return $"You've earned {earnedCoins} coins!";
        }

        public string CheckBalance(ulong userId, ulong guildId)
        {
            UserAndGuildConnection UnGConnection = GetUserAndGuildConnection(userId, guildId);
            Balance balance = GetBalance(UnGConnection.Id);

            if (UnGConnection is null)
            {                
                _dataProvider.CloseConnection();
                return "Error: You haven't earned coins on this server yet!";
            }          

            int cash = balance.AmountOfCoinsInCash;
            int dep = balance.AmountOfCoinsOnDeposit;

            _dataProvider.CloseConnection();

            return $"Cash: {cash} coin(s)\nOn deposit: {dep} coins(s)";
        }

        public abstract string AddIncome(ulong guildId, string roleName, int income);        
        
        public string CheckIncomes(ulong guildId)
        {
            _dataProvider.OpenConnection();

            if (!DatabaseController.Contexts.RnGConnections
                .Any(connection => connection.GuildId == $"{guildId}"))
            {                
                _dataProvider.CloseConnection();
                return "No roles with income on this server!";
            }

            string replyMessage = "";
            foreach (RoleAndGuildConnection connection in DatabaseController.Contexts.RnGConnections
                .Where(connection => connection.GuildId == $"{guildId}"))
            {
                replyMessage += $"{connection.RoleName} - {connection.Income} coin(s)\n";
            }                       

            _dataProvider.CloseConnection();

            return replyMessage;
        }
        
        public string GetIncomes(ulong userId, ulong guildId)
        {
            _dataProvider.OpenConnection();

            if (!DatabaseController.Contexts.RnGConnections
                .Any(connection => connection.GuildId == $"{guildId}"))
            {                
                return "No roles with income on this server!";
            }

            AddUserIfNotAdded(userId, guildId);

            UserAndGuildConnection UnGConnection = GetUserAndGuildConnection(userId, guildId);
            Balance balance = GetBalance(UnGConnection.Id);

            double timePassedFromLastEarning = (DateTime.Now - UnGConnection.RoleIncomeTime).TotalMinutes;

            if (timePassedFromLastEarning < TIMESPAN_FOR_INCOME)
            {                
                _dataProvider.CloseConnection();
                return $"Wait {((int)TIMESPAN_FOR_INCOME - (int)timePassedFromLastEarning) / 60} hours" +
                    $" {((int)TIMESPAN_FOR_INCOME - (int)timePassedFromLastEarning) % 60} minutes!";
            }

            string replyMessage = "";
            foreach (RoleAndGuildConnection connection in DatabaseController.Contexts.RnGConnections
                .Where(connection => connection.GuildId == $"{guildId}"))
            {
                replyMessage += $"You've collected income for {connection.RoleName} - {connection.Income} coin(s)\n";
                balance.AmountOfCoinsInCash += connection.Income;
            }

            UnGConnection.RoleIncomeTime = DateTime.Now;

            _databaseController.SaveContexts();   
            _dataProvider.CloseConnection();

            return replyMessage;
        }
        
        public string PutOnDeposit(ulong userId, ulong guildId, object value)
        {
            _dataProvider.OpenConnection();

            if (!int.TryParse(value.ToString(), out int amountOfCoinsToPut) && 
                value.ToString().Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                _dataProvider.CloseConnection();
                return "Error: You have to enter a number or \"all\"";
            }

            UserAndGuildConnection UnGConnection = GetUserAndGuildConnection(userId, guildId);
            Balance balance = GetBalance(UnGConnection.Id);

            if (UnGConnection is null)
            {                
                _dataProvider.CloseConnection();
                return "Error: You haven't earned coins on this server yet!";
            }

            if (balance.AmountOfCoinsInCash == 0)
            {                
                _dataProvider.CloseConnection();
                return "Error: You have no coins in cash!";
            }

            if (value.ToString().Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                amountOfCoinsToPut = balance.AmountOfCoinsInCash;
            }

            if (amountOfCoinsToPut < 0)
            {
                return "Error: You have to enter a positive number!";
            }

            if (balance.AmountOfCoinsInCash < amountOfCoinsToPut)
            {                
                _dataProvider.CloseConnection();
                return "Error: You don't have enough coins in cash!";
            }

            balance.AmountOfCoinsOnDeposit += amountOfCoinsToPut;
            balance.AmountOfCoinsInCash -= amountOfCoinsToPut;
            
            _databaseController.SaveContexts();
            _dataProvider.CloseConnection();

            return "You've put your coins on deposit!";
        }

        public string TakeFromDeposit(ulong userId, ulong guildId, object value)
        {
            _dataProvider.OpenConnection();

            if (!int.TryParse(value.ToString(), out int amountOfCoinsToPut) &&
                value.ToString().Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                _dataProvider.CloseConnection();
                return "Error: You have to enter a positive number or \"all\"";
            }

            UserAndGuildConnection UnGConnection = GetUserAndGuildConnection(userId, guildId);
            Balance balance = GetBalance(UnGConnection.Id);

            if (UnGConnection is null)
            {
                _dataProvider.CloseConnection();
                return "Error: You haven't earned coins on this server yet!";
            }

            if (balance.AmountOfCoinsOnDeposit == 0)
            {
                _dataProvider.CloseConnection();
                return "Error: You have no coins on deposit!";
            }

            if (value.ToString().Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                amountOfCoinsToPut = balance.AmountOfCoinsInCash;
            }

            if (amountOfCoinsToPut < 0)
            {
                return "Error: You have to enter a positive number!";
            }

            if (balance.AmountOfCoinsOnDeposit < amountOfCoinsToPut)
            {                
                _dataProvider.CloseConnection();
                return "Error: You don't have enough coins on deposit!";
            }

            balance.AmountOfCoinsInCash += amountOfCoinsToPut;
            balance.AmountOfCoinsOnDeposit -= amountOfCoinsToPut;            

            _databaseController.SaveContexts();
            _dataProvider.CloseConnection();

            return "You've taken your coins from deposit!";
        } 
        
        public string VanishCoins(ulong userId, ulong guildId)
        {
            _dataProvider.OpenConnection();

            UserAndGuildConnection UnGConnection = GetUserAndGuildConnection(userId, guildId);
            Balance balance = GetBalance(UnGConnection.Id);

            if (UnGConnection is null)
            {                
                _dataProvider.CloseConnection();
                return "Error: User hasn't earned coins on this server yet!";
            }   

            balance.AmountOfCoinsOnDeposit = 0;
            balance.AmountOfCoinsInCash = 0;

            _databaseController.SaveContexts();
            _dataProvider.CloseConnection();

            return $"Coins have been vanished!";
        }
        
        public virtual string AddCoins(ulong userId, ulong guildId, int value)
        {
            _dataProvider.OpenConnection();

            if (value < 0)
            {
                return "Error: You have to enter a positive number!";
            }

            AddUserIfNotAdded(userId, guildId);

            UserAndGuildConnection UnGConnection = GetUserAndGuildConnection(userId, guildId);

            Balance balance = GetBalance(UnGConnection.Id);

            balance.AmountOfCoinsInCash += value;
            
            _databaseController.SaveContexts();
            _dataProvider.CloseConnection();

            return $"Coins added: {value}";
        }
        
        public string Rob(ulong senderId, ulong userToRobId, ulong guildId)
        {
            _dataProvider.OpenConnection();

            AddUserIfNotAdded(senderId, guildId);

            UserAndGuildConnection UnGConnectionToRob = GetUserAndGuildConnection(userToRobId, guildId);
            UserAndGuildConnection UnGConnectionToAdd = GetUserAndGuildConnection(senderId, guildId);
            Balance balanceToRob = GetBalance(UnGConnectionToRob.Id);
            Balance balanceToAdd = GetBalance(UnGConnectionToAdd.Id);

            if (UnGConnectionToRob is null)
            {
                _dataProvider.CloseConnection();
                return "Error: User hasn't earned coins on this server yet!";
            }           

            if (balanceToRob.AmountOfCoinsInCash == 0)
            {                
                _dataProvider.CloseConnection();
                return "Error: User doesn't have coins in cash!";
            }

            int amountOfLootedCoins = Random.Shared.Next(0, balanceToRob.AmountOfCoinsInCash);

            balanceToRob.AmountOfCoinsInCash -= amountOfLootedCoins;
            balanceToAdd.AmountOfCoinsInCash += amountOfLootedCoins;

            _databaseController.SaveContexts();  
            _dataProvider.CloseConnection();
            return $"You successfully robbed {amountOfLootedCoins} coin(s)!";
        }
        
        public string PlayRoulette(ulong userId, ulong guildId, object bet, object betCoins)
        {
            _dataProvider.OpenConnection();

            UserAndGuildConnection UnGConnection = GetUserAndGuildConnection(userId, guildId);
            Balance balance = GetBalance(UnGConnection.Id);

            if (UnGConnection is null)
            {
                _dataProvider.CloseConnection();
                return "Error: You haven't earned coins on this server yet!";
            }

            if (!int.TryParse(betCoins.ToString(), out int amountOfCoinsToBet) &&
                !betCoins.ToString().Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                _dataProvider.CloseConnection();
                return "Error: You have to enter a positive number or \"all\"";
            }

            if (betCoins.ToString().Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                amountOfCoinsToBet = balance.AmountOfCoinsInCash;
            }

            if (amountOfCoinsToBet < 0)
            {
                return "Error: You have to enter a positive number!";
            }

            if (!int.TryParse(bet.ToString(), out int betNumber) && 
                !bet.ToString().Equals("red", StringComparison.OrdinalIgnoreCase) && 
                !bet.ToString().Equals("black", StringComparison.OrdinalIgnoreCase)) 
            {
                _dataProvider.CloseConnection();
                return "Error: You can only bet on red, black or a positive number less than 37!";
            }

            if (amountOfCoinsToBet < ROULETTE_MINIMAL_BET)
            {
                _dataProvider.CloseConnection();   
                return "Error: Minimal bet is 1!";
            }

            if (balance.AmountOfCoinsInCash < amountOfCoinsToBet)
            {                
                _dataProvider.CloseConnection();
                return "Error: You don't have enough coins in cash!";
            }

            if (bet.ToString().Equals("red", StringComparison.OrdinalIgnoreCase) || 
                bet.ToString().Equals("black", StringComparison.OrdinalIgnoreCase))
            {
                string betColour = bet.ToString();
                int colourRouletteResult = Random.Shared.Next(0, 36);
                string resultColour = IsRouletteNumberRed(colourRouletteResult) ? "red" : "black";
                
                if (resultColour == betColour)
                {
                    balance.AmountOfCoinsInCash += amountOfCoinsToBet * ROULETTE_COLOUR_WIN_MULTIPLIER;

                    _databaseController.SaveContexts();                    
                    _dataProvider.CloseConnection();
                    return $"Congratulations! You won {amountOfCoinsToBet * ROULETTE_COLOUR_WIN_MULTIPLIER}";
                }

                _databaseController.SaveContexts();
                _dataProvider.CloseConnection();
                return $"You lost! The ball landed on {colourRouletteResult}!";
            }

            if (betNumber > 36 && betNumber < 0)
            {                
                _dataProvider.CloseConnection();
                return "Error: You can only bet on red, black or a positive number less than 37!";
            }

            balance.AmountOfCoinsInCash -= amountOfCoinsToBet;

            int numberRouletteResult = Random.Shared.Next(0, 36);

            if (numberRouletteResult == betNumber && numberRouletteResult == 0)
            {
                balance.AmountOfCoinsInCash += amountOfCoinsToBet * ROULETTE_ZERO_WIN_MULTIPLIER;

                _databaseController.SaveContexts();               
                _dataProvider.CloseConnection();
                return $"Congratulations! You've won {amountOfCoinsToBet * ROULETTE_ZERO_WIN_MULTIPLIER}";
            }

            if (numberRouletteResult == betNumber)
            {
                balance.AmountOfCoinsInCash += amountOfCoinsToBet * ROULETTE_NUMBER_WIN_MULTIPLIER;

                _databaseController.SaveContexts();                
                _dataProvider.CloseConnection();
                return $"Congratulations! You've won {amountOfCoinsToBet * ROULETTE_NUMBER_WIN_MULTIPLIER}";
            }

            _databaseController.SaveContexts();
            _dataProvider.CloseConnection();

            return $"You've lost! The ball landed on {numberRouletteResult}!";
        }        
                
        public string CheckLeaderboard(ulong userId, ulong guildId, string option = "")
        {
            _dataProvider.OpenConnection();

            IEnumerable<UserAndGuildConnection> UnGConnections =
                DatabaseController.Contexts.UnGConnections.Where(connection => connection.GuildId == $"{guildId}");
            List<Balance> balances = new List<Balance>();

            if (!UnGConnections.Any())
            {                
                _dataProvider.CloseConnection();
                return "Error: There is no players who've earned coins on this server!";
            }

            string leaderboardString = $"**{guildId}**'s guild leaderboard:\n";

            foreach (UserAndGuildConnection connection in UnGConnections)
            {
                Balance balanceToAdd = DatabaseController.Contexts.Balances
                        .Where(balance => balance.IdOfUnGConnection == connection.Id)
                        .First();
                balances.Add(balanceToAdd);
            }

            if (!option.Equals("cash", StringComparison.OrdinalIgnoreCase) &&
                !option.Equals("dep", StringComparison.OrdinalIgnoreCase) && 
                !option.Equals("deposit", StringComparison.OrdinalIgnoreCase) &&
                !option.Equals(string.Empty))
            {                
                _dataProvider.CloseConnection();
                return "Error: You have to enter \"cash\", \"dep\" or \"deposit\"!";
            }

            IOrderedEnumerable<Balance> orderedBalances;

            if (option.Equals("cash", StringComparison.OrdinalIgnoreCase))
            {
                orderedBalances = balances.OrderByDescending(balance => balance.AmountOfCoinsInCash);
            }
            else if (option.Equals("dep", StringComparison.OrdinalIgnoreCase) ||
                option.Equals("deposit", StringComparison.OrdinalIgnoreCase))
            {
                orderedBalances = balances.OrderByDescending(balance => balance.AmountOfCoinsOnDeposit);
            }
            else
            {
                orderedBalances = 
                    balances.OrderByDescending(balance => balance.AmountOfCoinsOnDeposit + balance.AmountOfCoinsInCash);
            }

            for (int i = 0; i < LEADERBOARD_SIZE; i++)
            {
                if (i >= orderedBalances.Count())
                {
                    break;
                }

                Balance currentBalance = orderedBalances.ElementAt(i);
                int idOfUnGConnection = currentBalance.IdOfUnGConnection;
                string currentUserId = DatabaseController.Contexts.UnGConnections
                    .Where(connection => connection.Id == idOfUnGConnection)
                    .First().UserId;                

                leaderboardString +=
                    $"{i + 1}. {currentUserId} —" +
                    $" {currentBalance.AmountOfCoinsInCash + currentBalance.AmountOfCoinsOnDeposit}\n";
            }

            _dataProvider.CloseConnection();
            return leaderboardString;
        }
                
        public abstract object[] PlayBlackJack(ulong userId, ulong guildId, object betCoins);

        public abstract string BuyRole(ulong userId, ulong guildId, string roleName);

        private static void AddUserIfNotAdded(ulong userId, ulong guildId)
        {
            if (!DatabaseController.Contexts.Users.Any(user => user.Id == $"{userId}"))
            {
                DatabaseController.Contexts.Users.Add(new User { Id = $"{userId}" });
            }

            if (!DatabaseController.Contexts.Guilds.Any(guild => guild.Id == $"{guildId}"))
            {
                DatabaseController.Contexts.Guilds.Add(new Guild { Id = $"{guildId}" });
            }

            if (!DatabaseController.Contexts.UnGConnections.Any(connection =>
                connection.UserId == $"{userId}" &&
                connection.GuildId == $"{guildId}"))
            {
                DatabaseController.Contexts.UnGConnections.Add(
                    new UserAndGuildConnection
                    {
                        Id = DatabaseController.Contexts.UnGConnections.Count,
                        UserId = $"{userId}",
                        GuildId = $"{guildId}",
                        EarnTime = DateTime.MinValue,
                        RoleIncomeTime = DateTime.MinValue
                    });
            }

            int idOfUserAndGuildConnection = DatabaseController.Contexts.UnGConnections
                .Where(connection =>
                    connection.UserId == $"{userId}" &&
                    connection.GuildId == $"{guildId}")
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

        private static bool IsRouletteNumberRed(int number)
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

        private static UserAndGuildConnection GetUserAndGuildConnection(ulong userId, ulong guildId)
        {
            return DatabaseController.Contexts.UnGConnections
                    .Where(connection =>
                        connection.UserId == $"{userId}" &&
                        connection.GuildId == $"{guildId}")
                    .First();
        }

        private static Balance GetBalance(int idOfUnGConnection)
        {
            return DatabaseController.Contexts.Balances
                    .Where(balance => balance.IdOfUnGConnection == idOfUnGConnection)
                    .First();
        }
    }
}
