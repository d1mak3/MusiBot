using Newtonsoft.Json.Linq;

namespace MusiBotProd.Utilities.Data
{
    public interface IModel
    {
        JObject ToJObject();
        void FromJObject(JObject jObjectToConvertFrom);
    }
}
