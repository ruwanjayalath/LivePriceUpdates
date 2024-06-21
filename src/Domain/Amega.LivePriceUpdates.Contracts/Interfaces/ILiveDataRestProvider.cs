using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amega.LivePriceUpdates.Contracts.Interfaces
{
    public interface ILiveDataRestProvider
    {
        public string Name { get; }
        Task<List<LiveQuote>> GetLiveQuotes(List<string> symbols);
    }
}
