using Newtonsoft.Json.Linq;

namespace MusiBotProd.Utilities.Data.Models
{
    public class UserAndGuildConnection : IModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string GuildId { get; set; }
        public DateTime EarnTime { get; set; }
        public DateTime RoleIncomeTime { get; set; }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();

            jObject["0"] = Id;
            jObject["1"] = UserId;
            jObject["2"] = GuildId;
            jObject["3"] = EarnTime.ToString("O");
            jObject["4"] = RoleIncomeTime.ToString("O");

            return jObject;
        }

        public void FromJObject(JObject jObjectToConvertFrom)
        {
            Id = (int) jObjectToConvertFrom["0"];
            UserId = (string) jObjectToConvertFrom["1"];
            GuildId = (string) jObjectToConvertFrom["2"];
            EarnTime = (DateTime) jObjectToConvertFrom["3"];
            RoleIncomeTime = (DateTime) jObjectToConvertFrom["4"];
        }
    }
}
