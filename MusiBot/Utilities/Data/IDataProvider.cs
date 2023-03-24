using MusiBotProd.Utilities.Data.Models;
using Newtonsoft.Json.Linq;

namespace MusiBotProd.Utilities.Data
{
    /// <summary>
    /// Data providers description
    /// </summary>
    public interface IDataProvider
    {
        #region API methods description

        public void OpenConnection();
        public void CloseConnection();
        public void SaveBalance(Balance balance);
        public void SaveGuild(Guild guild);
        public void SaveRnGConnection(RoleAndGuildConnection connection);
        public void SaveUser(User user);
        public void SaveUnGConnection(UserAndGuildConnection connection);       
        public List<JObject> Get(string dbName, int dbColumnsCount);

        #endregion
    }
}
