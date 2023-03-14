using Newtonsoft.Json.Linq;

namespace MusiBotProd.Utilities.Data
{
    /// <summary>
    /// Models description
    /// </summary>
    public interface IModel
    {
        #region models methods description

        JObject ToJObject();
        void FromJObject(JObject jObjectToConvertFrom);

        #endregion
    }
}