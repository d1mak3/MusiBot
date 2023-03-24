using MusiBotProd.Utilities.Data.Models;
using Newtonsoft.Json.Linq;

namespace MusiBotProd.Utilities.Data
{
    /// <summary>
    /// Database controller which interacts with database API
    /// </summary>
    public class DatabaseController
    {
        #region readonlies

        private readonly IDataProvider dataProvider;

        #endregion

        #region statics

        public static DataContexts Contexts { get; private set; }

        #endregion

        #region constructors

        public DatabaseController(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;

            dataProvider.OpenConnection();

            if (Contexts == null)
            {
                Contexts = new DataContexts();
                GetContexts();
            }

            dataProvider.CloseConnection();
        }

        #endregion

        #region API interaction methods

        public void SaveContexts()
        {
            foreach (Balance balance in Contexts.Balances)
            {
                dataProvider.SaveBalance(balance);
            }

            foreach (Guild guild in Contexts.Guilds)
            {
                dataProvider.SaveGuild(guild);
            }

            foreach (User user in Contexts.Users)
            {
                dataProvider.SaveUser(user);
            }

            foreach (UserAndGuildConnection connection in Contexts.UnGConnections)
            {
                dataProvider.SaveUnGConnection(connection);
            }

            foreach (RoleAndGuildConnection connection in Contexts.RnGConnections)
            {
                dataProvider.SaveRnGConnection(connection);
            }
        }

        #endregion

        #region supporting methods

        private void GetContexts()
        {
            if (Contexts.Balances == null)
            {
                Contexts.Balances = new List<Balance>();
            }

            foreach (JObject jo in dataProvider.Get("balance", 4))
            {
                Balance balance = new Balance();
                balance.FromJObject(jo);

                Contexts.Balances.Add(balance);
            }

            if (Contexts.Guilds == null)
            {
                Contexts.Guilds = new List<Guild>();
            }

            foreach (JObject jo in dataProvider.Get("guild", 1))
            {
                Guild guild = new Guild();
                guild.FromJObject(jo);

                Contexts.Guilds.Add(guild);
            }

            if (Contexts.RnGConnections == null)
            {
                Contexts.RnGConnections = new List<RoleAndGuildConnection>();
            }

            foreach (JObject jo in dataProvider.Get("role_and_guild_connection", 4))
            {
                RoleAndGuildConnection RnGConnection = new RoleAndGuildConnection();
                RnGConnection.FromJObject(jo);

                Contexts.RnGConnections.Add(RnGConnection);
            }

            if (Contexts.Users == null)
            {
                Contexts.Users = new List<User>();
            }

            foreach (JObject jo in dataProvider.Get("user", 1))
            {
                User user = new User();
                user.FromJObject(jo);

                Contexts.Users.Add(user);
            }

            if (Contexts.UnGConnections == null)
            {
                Contexts.UnGConnections = new List<UserAndGuildConnection>();
            }

            foreach (JObject jo in dataProvider.Get("user_and_guild_connection", 5))
            {
                UserAndGuildConnection UnGConnection = new UserAndGuildConnection();
                UnGConnection.FromJObject(jo);

                Contexts.UnGConnections.Add(UnGConnection);
            }
        }

        #endregion
    }
}
