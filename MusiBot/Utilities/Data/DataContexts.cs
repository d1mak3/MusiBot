namespace MusiBotProd.Utilities.Data
{
    public class DataContexts
    {
        public List<Models.Balance> Balances { get; set; }
        public List<Models.Guild> Guilds { get; set; }
        public List<Models.RoleAndGuildConnection> RnGConnections { get; set; }
        public List<Models.User> Users { get; set; }
        public List<Models.UserAndGuildConnection> UnGConnections { get; set; }
        
    }
}
