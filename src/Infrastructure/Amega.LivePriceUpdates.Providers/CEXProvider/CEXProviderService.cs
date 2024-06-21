using Amega.LivePriceUpdates.Contracts;
using Amega.LivePriceUpdates.Contracts.Configuration;
using Amega.LivePriceUpdates.Contracts.Interfaces;
using Amega.LivePriceUpdates.Providers.AlphaVantageProvider;
using Amega.LivePriceUpdates.Providers.CEXProviderModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Amega.LivePriceUpdates.Providers.CEXProvider
{
    public class CEXProviderService : ILiveDataWebSocketProvider
    {
        private const int CHUNK_SIZE = 50000;
        private readonly ProviderConfiguration _configuration;
        private readonly ILogger<CEXProviderService> _logger;
        private ClientWebSocket _ws;

        public string Name => "CEX";
        public event Func<List<LiveQuote>, Task> PriceUpdateReceived;        

        public CEXProviderService(IOptions<ProviderConfiguration> options, ILogger<CEXProviderService> logger)
        {
            _configuration = options.Value;
            _logger = logger;
        }

        public async Task StartConsumer(List<string> supportedSymbols)
        {
            _ws = new ClientWebSocket();
            byte[] buffer = new byte[CHUNK_SIZE];

            await _ws.ConnectAsync(new Uri(_configuration.CEXWebsocketUrl), CancellationToken.None);
            _logger.LogInformation($"{nameof(CEXProviderService)} - Websocket connected successfully.");           

            var sendTickersTask = Task.Run(async () =>
            {
                var getTickersMessage = GetTickerMessage(supportedSymbols);
                var bytes = Encoding.UTF8.GetBytes(getTickersMessage);

                _logger.LogInformation($"{nameof(CEXProviderService)} - Sending get ticker information message");
                await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            });

            var keepAliveTask = Task.Run(async () =>
            {
                while (true)
                {
                    var bytes = Encoding.UTF8.GetBytes(@"{""e"":""ping""}");
                    await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

                    await Task.Delay(_configuration.CEXKeepAliveInterval);
                }
            });

            var receiveTask = Task.Run(async () =>
            {
                while (_ws.State == WebSocketState.Open)
                {
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    ProcessMessage(message, supportedSymbols);
                }
            });

            await keepAliveTask;
            await sendTickersTask;
            await receiveTask;
        }

        private string GetTickerMessage(List<string> supportedSymbols)
        {
            var symbols = GenerateSymbolPairs(supportedSymbols);

            return @"{
              ""e"": ""get_ticker"",
              ""oid"": ""10181762572482_get_ticker"",
              ""data"": {
                ""pairs"": [" + symbols + @"]
              }
            }";
        }

        private string TradeSubscribeMessage(string symbol)
        {
            return @"{
              ""e"": ""trade_subscribe"",
              ""oid"": ""72955210375_trade_subscribe"",
              ""data"": {
                ""pair"": " + $"\"{symbol}\"" + @"
              }
            }";
        }

        private void ProcessMessage(string message, List<string> supportedSymbols)
        {
            ParseJsonMessage(message, supportedSymbols);
        }

        public async void StopConsumer()
        {
            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing web socket client", CancellationToken.None);
        }

        private List<LiveQuote> ParseJsonMessage(string json, List<string> availableSymbols)
        {
            dynamic jsonMessage = JsonConvert.DeserializeObject<ExpandoObject>(json)!;

            var type = jsonMessage.e.ToString();

            switch (type.ToLower())
            {
                case "tradehistorysnapshot":
                    ParseTradeHistory(json);
                    break;

                case "tradeupdate":
                    ParseTradeUpdate(json);
                    break;

                case "get_ticker":
                    ParseTickerInfo(json);
                    break;

                default:
                    break;
            }

            return new List<LiveQuote>();
        }

        private void ParseTickerInfo(string json)
        {
            List<string> providerSupportedSymbols = new List<string>();
            JObject jsonMessage = JObject.Parse(json);

            var dataList = jsonMessage["data"];

            foreach (dynamic item in dataList)
            {
                var pair = item.First;

                if (pair["error"] == null)
                    providerSupportedSymbols.Add(item.Name);
            }

            SubscribeToProviderSupportedSymbols(providerSupportedSymbols);
        }

        private async void SubscribeToProviderSupportedSymbols(List<string> providerSupportedSymbols)
        {
            var subscribeTask = Task.Run(async () =>
            {
                foreach (var symbol in providerSupportedSymbols)
                {
                    var message = TradeSubscribeMessage(symbol);

                    var bytes = Encoding.UTF8.GetBytes(message);
                    await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

                    _logger.LogInformation($"{nameof(CEXProviderService)} - Succesfully subscribed for symbol :{symbol}");
                }
            });

            await subscribeTask;
        }

        private void ParseTradeHistory(string json)
        {
            var parsed = JsonConvert.DeserializeObject<TradeHistorySnapshot>(json);

            if (parsed == null)
                return;

            var lastTrade = parsed.Data.Trades.OrderByDescending(x => x.DateISO).FirstOrDefault();

            var results = new List<LiveQuote>()
            {

                new LiveQuote()
                {
                    LastUpdated = lastTrade.DateISO,
                    Source = Name,
                    Symbol = parsed.Data.Pair.Replace("-",""),
                    Price = lastTrade.Price.ToString()
                }
            };

            _logger.LogInformation($"{nameof(CEXProviderService)} - Updating cache based on trade history. Data : {results}");
            PriceUpdateReceived?.Invoke(results);
        }

        private void ParseTradeUpdate(string json)
        {
            var parsed = JsonConvert.DeserializeObject<TradeUpdate>(json);

            if (parsed == null)
                return;

            var results = new List<LiveQuote>()
            {

                new LiveQuote()
                {
                    LastUpdated = parsed.Data.DateISO,
                    Source = Name,
                    Symbol = parsed.Data.Pair.Replace("-",""),
                    Price = parsed.Data.Price.ToString()
                }
            };

            _logger.LogInformation($"{nameof(CEXProviderService)} - Updating cache based on trade update. Data : {results}");
            PriceUpdateReceived?.Invoke(results);
        }

        private string GenerateSymbolPairs(List<string> supportedSymbols)
        {
            List<string> symbolPairs = [];

            foreach (var symbol in supportedSymbols)
            {
                if (symbol.Length < 6)
                    continue;

                symbolPairs.Add($"{symbol.Substring(0, 3)}-{symbol.Substring(3, 3)}");
            }

            return string.Join(", ", symbolPairs.Select(x => $"\"{x}\""));
        }
    }
}
