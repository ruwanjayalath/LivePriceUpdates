﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amega.LivePriceUpdates.Contracts
{
    public class LiveQuote
    {
        public string Symbol { get; set; }
        public string Price { get; set; }
        public string Source { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
