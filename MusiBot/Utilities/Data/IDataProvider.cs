using MusiBotProd.Utilities.Data.Models;
using Newtonsoft.Json.Linq;

namespace MusiBotProd.Utilities.Data
{
    public interface IDataProvider
    {
        public void OpenConnection();

        public void SaveBalance(Balance balance);

        public void SaveGuild(Guild guild);

        public void SaveRnGConnection(RoleAndGuildConnection connection);

        public void SaveUser(User user);

        public void SaveUnGConnection(UserAndGuildConnection connection);
       
        public List<JObject> Get(string dbName, int dbColumnsCount);       
    }
}
