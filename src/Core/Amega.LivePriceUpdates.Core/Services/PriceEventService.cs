using Amega.LivePriceUpdates.Contracts;
using Amega.LivePriceUpdates.Contracts.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amega.LivePriceUpdates.Core.Services
{
    public interface IPriceEventService
    {
        Task<List<LiveQuote>> GetLiveQuotes(List<string> symbols);
        Task UpdateCache(List<LiveQuote> liveQuotes);

        List<string> GetSupportedSymbols();
    }

    public class PriceEventService : IPriceEventService
    {
        private readonly LivePriceUpdateConfiguration _configuration;
        private readonly ILogger<PriceEventService> _logger;
        private readonly IMemoryCache _memoryCache;

        public PriceEventService(IOptions<LivePriceUpdateConfiguration> options, ILogger<PriceEventService> logger, IMemoryCache memoryCache)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _configuration = options.Value;
        }

        public Task<List<LiveQuote>> GetLiveQuotes(List<string> symbols)
        {
            List<LiveQuote> results = [];

            foreach (var symbol in symbols)
            {
                var supportedSymbol = _configuration.SupportedSymbols.FirstOrDefault(x => x == symbol);

                if (supportedSymbol == null)
                    continue;

                if (_memoryCache.TryGetValue(supportedSymbol, out object cacheObj))
                {
                    if (cacheObj == null)
                        continue;

                    var quote = (LiveQuote)cacheObj;
                    results.Add(quote);
                }
            }

            return Task.FromResult(results);
        }

        public List<string> GetSupportedSymbols()
        {
            return _configuration.SupportedSymbols;
        }

        public async Task UpdateCache(List<LiveQuote> liveQuotes)
        {
            foreach (var quote in liveQuotes)
            {
                _memoryCache.Set<LiveQuote>(quote.Symbol, quote);
            }

            await Task.CompletedTask;
        }
    }
}
