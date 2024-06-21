using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Amega.LivePriceUpdates.Core.Services
{
    public interface IHttpService
    {
        Task<string> GetAsync(string uri);
    }

    public class HttpService : IHttpService
    {
        private readonly HttpClient _client;

        public HttpService()
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All
            };

            _client = new HttpClient(handler);
        }

        public async Task<string> GetAsync(string uri)
        {
            using HttpResponseMessage response = await _client.GetAsync(uri);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
