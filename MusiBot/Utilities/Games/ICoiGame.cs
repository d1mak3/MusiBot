namespace MusiBotProd.Utilities.Games
{
    public interface ICoiGame
    {
        public string EarnCoins(ulong userId, ulong guildId);
        public string CheckBalance(ulong userId, ulong guildId);
        public string AddIncome(ulong guildId, string roleName, int income);
        public string CheckIncomes(ulong guildId);
        public string GetIncomes(ulong userId, ulong guildId);
        public string PutOnDeposit(ulong userId, ulong guildId, object value);
        public string TakeFromDeposit(ulong userId, ulong guildId, object value);
        public string VanishCoins(ulong userId, ulong guildId);
        public string AddCoins(ulong userId, ulong guildId, int value);
        public string Rob(ulong senderId, ulong userToRobId, ulong guildId);
        public string PlayRoulette(ulong userId, ulong guildId, object bet, object betCoins);
        public string CheckLeaderboard(ulong userId, ulong guildId, string option = "");
        public object[] PlayBlackJack(ulong userId, ulong guildId, object betCoins);
        public string BuyRole(ulong userId, ulong guildId, string roleName);
    }
}
