namespace MusiBotProd.Utilities.Data
{
    /// <summary>
    /// Collections of models
    /// </summary>
    public class DataContexts
    {
        #region collections

        public List<Models.Balance> Balances { get; set; }
        public List<Models.Guild> Guilds { get; set; }
        public List<Models.RoleAndGuildConnection> RnGConnections { get; set; }
        public List<Models.User> Users { get; set; }
        public List<Models.UserAndGuildConnection> UnGConnections { get; set; }

        #endregion
    }
}
