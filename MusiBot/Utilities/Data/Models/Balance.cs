﻿using Newtonsoft.Json.Linq;

namespace MusiBotProd.Utilities.Data.Models
{
    /// <summary>
    /// Balance entity
    /// </summary>
    public class Balance : IModel
    {
        #region data fields

        public int Id { get; set; }
        public int IdOfUnGConnection { get; set; }
        public int AmountOfCoinsOnDeposit { get; set; }
        public int AmountOfCoinsInCash { get; set; }

        #endregion

        #region supporting methods

        public JObject ToJObject()
        {
            JObject jObject = new JObject();

            jObject["0"] = Id;
            jObject["1"] = IdOfUnGConnection;
            jObject["2"] = AmountOfCoinsOnDeposit;
            jObject["3"] = AmountOfCoinsInCash;

            return jObject;
        }

        public void FromJObject(JObject jObjectToConvertFrom)
        {
            Id = (int) jObjectToConvertFrom["0"];
            IdOfUnGConnection = (int) jObjectToConvertFrom["1"];
            AmountOfCoinsOnDeposit = (int) jObjectToConvertFrom["2"];
            AmountOfCoinsInCash = (int) jObjectToConvertFrom["3"];            
        }

        #endregion
    }
}
