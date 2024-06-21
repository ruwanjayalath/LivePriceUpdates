using Amega.LivePriceUpdates.Contracts.Configuration;
using Amega.LivePriceUpdates.Contracts.Interfaces;
using Amega.LivePriceUpdates.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Amega.LivePriceUpdates.API.BackgroundServices
{
    public class RestAPIConsumer : BackgroundService
    {
        private IServiceProvider _serviceProvider;
        private readonly ILogger<RestAPIConsumer> _logger;
        private readonly IEnumerable<ILiveDataRestProvider> _restDataProviders;
        private readonly ProviderConfiguration _providerConfiguration;

        public RestAPIConsumer(ILogger<RestAPIConsumer> logger
            , IServiceProvider serviceProvider
            , IEnumerable<ILiveDataRestProvider> restDataProviders
            , IOptions<ProviderConfiguration> options)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _restDataProviders = restDataProviders;
            _providerConfiguration = options.Value;
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

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        foreach (var provider in _restDataProviders)
                        {
                            var supportedSymbols = scopedPriceEventService.GetSupportedSymbols();

                            _logger.LogInformation($"{this.GetType()}.{nameof(ExecuteAsync)} - Getting live quotes from provider: {provider.Name}");
                            var results = await provider.GetLiveQuotes(supportedSymbols);

                            await scopedPriceEventService.UpdateCache(results);
                        }

                        await Task.Delay(TimeSpan.FromMinutes(_providerConfiguration.AlphaVantagePollingIntervalInMins), stoppingToken);
                    }

                    //Stop
                    _logger.LogInformation($"{this.GetType()}.{nameof(ExecuteAsync)} - Stopped BackgroundService");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(RestAPIConsumer)}.{nameof(ExecuteAsync)} - Error occurred on ExecuteAsync");
            }
        }
    }
}
