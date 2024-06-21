using Amega.LivePriceUpdates.Contracts.Configuration;
using Amega.LivePriceUpdates.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Unicode;

namespace Amega.LivePriceUpdates.API.Controllers
{
    public class WebSocketController : ControllerBase
    {
        private readonly IPriceEventService _priceEventService;
        private readonly LivePriceUpdateConfiguration _configuration;

        public WebSocketController(IOptions<LivePriceUpdateConfiguration> options, IPriceEventService priceEventService)
        {
            _priceEventService = priceEventService;
            _configuration = options.Value;
        }

        [Route("/ws")]
        [HttpGet]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var supportedSymbols = _configuration.SupportedSymbols;

                while (true)
                {
                    var results = _priceEventService.GetLiveQuotes(supportedSymbols);

                    var testMessage = JsonConvert.SerializeObject(results);
                    var bytes = Encoding.UTF8.GetBytes(testMessage);
                    var arraySegment = new ArraySegment<byte>(bytes);

                    if (webSocket.State == WebSocketState.Open)
                        webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);

                    else if (webSocket.State == WebSocketState.Aborted || webSocket.State == WebSocketState.Closed)
                        break;

                    await Task.Delay(_configuration.WebSocketBroadcastDelayInMiliSecs);
                }
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
    }
}
