using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amega.LivePriceUpdates.Contracts.Configuration
{
    public class LivePriceUpdateConfiguration
    {
        public const string SectionName = "LivePriceUpdates";

        public List<string> SupportedSymbols { get; set; }

        public int WebSocketBroadcastDelayInMiliSecs { get; set; }
    }
}
