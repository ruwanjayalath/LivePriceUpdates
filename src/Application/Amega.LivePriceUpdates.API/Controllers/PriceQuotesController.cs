using Amega.LivePriceUpdates.Contracts;
using Amega.LivePriceUpdates.Contracts.Configuration;
using Amega.LivePriceUpdates.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Amega.LivePriceUpdates.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriceQuotesController : ControllerBase
    {
        private readonly LivePriceUpdateConfiguration _configuration;
        private readonly IPriceEventService _priceEventService;

        public PriceQuotesController(IOptions<LivePriceUpdateConfiguration> options, IPriceEventService priceEventService)
        {
            _configuration = options.Value;
            _priceEventService = priceEventService;
        }

        [HttpPost("GetQuotes")]
        public async Task<IActionResult> GetLiveQuotes(List<string> symbols)
        {
            var results = _priceEventService.GetLiveQuotes(symbols);
            return Ok(results);
        }

        [HttpGet("GetSupportedSymbols")]
        public async Task<IActionResult> GetSupportedSymbols()
        {
            return Ok(_priceEventService.GetSupportedSymbols());
        }
    }
}
