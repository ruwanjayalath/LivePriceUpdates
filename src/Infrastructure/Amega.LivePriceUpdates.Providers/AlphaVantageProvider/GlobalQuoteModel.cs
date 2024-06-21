using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amega.LivePriceUpdates.Providers.AlphaVantageProvider
{
    internal class GlobalQuoteModel
    {
        [JsonProperty("Global Quote")]
        public Quote Quote { get; set; }
    }

    internal class Quote
    {
        [JsonProperty("01. symbol")]
        public string Symbol { get; set; }

        [JsonProperty("05. price")]
        public string Price { get; set; }

        [JsonProperty("07. latest trading day")]
        public DateTime LastTraded { get; set; }
    }
}
