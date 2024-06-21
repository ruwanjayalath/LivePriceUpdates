using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Amega.LivePriceUpdates.Providers.CEXProviderModels
{
    internal class TradeUpdateData
    {
        [JsonPropertyName("side")]
        public string Side { get; set; }

        [JsonPropertyName("pair")]
        public string Pair { get; set; }

        [JsonPropertyName("dateISO")]
        public DateTime DateISO { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("tradeId")]
        public string TradeId { get; set; }
    }

    internal class TradeUpdate
    {
        [JsonPropertyName("e")]
        public string Type { get; set; }

        [JsonPropertyName("data")]
        public TradeUpdateData Data { get; set; }
    }
}
