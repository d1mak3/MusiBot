using Newtonsoft.Json.Linq;

namespace MusiBotProd.Utilities.Data.Models
{
    public class User : IModel
    {
        public string Id { get; set; }        

        public JObject ToJObject()
        {
            JObject jObject = new JObject();

            jObject["0"] = Id;            

            return jObject;
        }

        public void FromJObject(JObject jObjectToConvertFrom)
        {
            Id = (string) jObjectToConvertFrom["0"];            
        }
    }
}
