using Newtonsoft.Json.Linq;

namespace MusiBotProd.Utilities.Data.Models
{
    public class RoleAndGuildConnection : IModel
    {
        public int Id { get; set; }       
        public string GuildId { get; set; }
        public string RoleName { get; set; }
        public int Income { get; set; }
        

        public JObject ToJObject()
        {
            JObject jObject = new JObject();

            jObject["0"] = Id;            
            jObject["1"] = GuildId;
            jObject["2"] = RoleName;
            jObject["3"] = Income;

            return jObject;
        }

        public void FromJObject(JObject jObjectToConvertFrom)
        {
            Id = (int) jObjectToConvertFrom["0"];
            GuildId = (string) jObjectToConvertFrom["1"];
            RoleName = (string) jObjectToConvertFrom["2"];
            Income = (int) jObjectToConvertFrom["3"];            
        }
    }
}
