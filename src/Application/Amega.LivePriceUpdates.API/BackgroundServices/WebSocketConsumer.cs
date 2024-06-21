using Amega.LivePriceUpdates.Contracts;
using Amega.LivePriceUpdates.Contracts.Configuration;
using Amega.LivePriceUpdates.Contracts.Interfaces;
using Amega.LivePriceUpdates.Core.Services;
using Microsoft.Extensions.Options;

namespace Amega.LivePriceUpdates.API.BackgroundServices
{
    public class WebSocketConsumer : BackgroundService
    {
        private readonly IEnumerable<ILiveDataWebSocketProvider> _webSocketProviders;
        private readonly ILogger<RestAPIConsumer> _logger;
        private readonly LivePriceUpdateConfiguration _configuration;
        private IServiceProvider _serviceProvider;

        public WebSocketConsumer(IEnumerable<ILiveDataWebSocketProvider> webSocketProviders
            , IServiceProvider serviceProvider
            , ILogger<RestAPIConsumer> logger
            , IOptions<LivePriceUpdateConfiguration> options)
        {
            _logger = logger;
            _configuration = options.Value;
            _serviceProvider = serviceProvider;
            _webSocketProviders = webSocketProviders;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var scopedPriceEventService =
                        scope.ServiceProvider
                            .GetRequiredService<IPriceEventService>();

                    //Initialize
                    _logger.LogInformation($"{this.GetType()}.{nameof(ExecuteAsync)} - Initializing BackgroundService");

                    foreach (var provider in _webSocketProviders)
                    {
                        var supportedSymbols = scopedPriceEventService.GetSupportedSymbols();

                        _logger.LogInformation($"{this.GetType()}.{nameof(ExecuteAsync)} - Getting live quotes from provider: {provider.Name}");

                        provider.PriceUpdateReceived += ProviderPriceUpdateReceived;
                        await provider.StartConsumer(_configuration.SupportedSymbols);
                    }

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    }

                    //Stop
                    _logger.LogInformation($"{this.GetType()}.{nameof(ExecuteAsync)} - Stopped BackgroundService");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(WebSocketConsumer)}.{nameof(ExecuteAsync)} - Error occurred on ExecuteAsync");
            }
        }

        private async Task ProviderPriceUpdateReceived(List<LiveQuote> liveQuotes)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var scopedPriceEventService =
                    scope.ServiceProvider
                        .GetRequiredService<IPriceEventService>();

                await scopedPriceEventService.UpdateCache(liveQuotes);
            }
        }
    }
}
