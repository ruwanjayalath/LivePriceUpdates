using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amega.LivePriceUpdates.Contracts.Configuration
{
    public class ProviderConfiguration
    {
        public const string SectionName = "ProviderConfiguration";

        public string AlphaVantageAPIKey { get; set; }
        public string GlobalQuoteAPIUrl { get; set; }
        public string CEXWebsocketUrl { get; set; }
        public int CEXKeepAliveInterval { get; set; }
        public int AlphaVantagePollingIntervalInMins { get; set; }
    }
}
