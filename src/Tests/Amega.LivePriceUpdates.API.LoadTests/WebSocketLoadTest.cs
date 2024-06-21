using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Amega.LivePriceUpdates.API.LoadTests
{
    public class WebSocketLoadTest
    {
        private const string WEB_SOCKET_URL = "ws://localhost:5273/ws";

        [Theory]
        [InlineData(1000)]
        public void LoadTest(int noOfClients)
        {
            for (int i = 0; i < noOfClients; i++)
            {
                CreateClient().ConfigureAwait(false);

                Debug.WriteLine($"Client created :{i}");
            }
        }

        private async Task CreateClient()
        {
            var ws = new ClientWebSocket();

            await ws.ConnectAsync(new Uri(WEB_SOCKET_URL), CancellationToken.None);

            var receiveTask = Task.Run(async () =>
            {

            });

            await receiveTask;
        }
    }
}
