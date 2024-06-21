using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amega.LivePriceUpdates.Contracts.Interfaces
{
    public interface ILiveDataWebSocketProvider
    {
        public string Name { get; }

        public event Func<List<LiveQuote>, Task> PriceUpdateReceived;

        Task StartConsumer(List<string> supportedSymbols);

        void StopConsumer();
    }
}
