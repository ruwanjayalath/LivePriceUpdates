using Amega.LivePriceUpdates.Contracts;
using Amega.LivePriceUpdates.Contracts.Configuration;
using Amega.LivePriceUpdates.Contracts.Interfaces;
using Amega.LivePriceUpdates.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amega.LivePriceUpdates.Providers.AlphaVantageProvider
{
    public class AlphaVantageProviderService : ILiveDataRestProvider
    {
        private readonly IHttpService _httpService;
        private readonly ProviderConfiguration _configuration;
        private readonly ILogger<AlphaVantageProviderService> _logger;

        public AlphaVantageProviderService(IHttpService httpService
            , IOptions<ProviderConfiguration> options
            , ILogger<AlphaVantageProviderService> logger)
        {
            _logger = logger;
            _httpService = httpService;
            _configuration = options.Value;
        }

        public string Name => "Alpha Vantage";

        public async Task<List<LiveQuote>> GetLiveQuotes(List<string> symbols)
        {
            List<LiveQuote> quotes = [];

            foreach (var symbol in symbols)
            {
                var url = GenerateUrl(symbol);
                _logger.LogInformation($"Performing API call to get global quotes. Symbol :{symbol}");

                var json = await _httpService.GetAsync(url);

                var providerQuote = JsonConvert.DeserializeObject<GlobalQuoteModel>(json);

                if (providerQuote == null)
                    continue;

                quotes.Add(new LiveQuote()
                {
                    Symbol = symbol,
                    LastUpdated = providerQuote.Quote.LastTraded,
                    Price = providerQuote.Quote.Price,
                    Source = $"Rest Provider:{Name}"
                });
            }

            return quotes;
        }

        private string GenerateUrl(string symbol)
          => $"{_configuration.GlobalQuoteAPIUrl}&apikey={_configuration.AlphaVantageAPIKey}&symbol={symbol}";
    }
}
