using Newtonsoft.Json.Linq;

namespace MusiBotProd.Utilities.Data.Models
{
    /// <summary>
    /// User entity
    /// </summary>
    public class User : IModel
    {
        #region data fields

        public string Id { get; set; }

        #endregion

        #region supporting methods

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

        #endregion
    }
}
