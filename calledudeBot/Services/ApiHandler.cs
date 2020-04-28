using calledudeBot.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;

namespace calledudeBot.Services
{
    public sealed class APIHandler<T> : IDisposable where T : BaseModel
    {
        private string _url;
        private readonly Timer _timer;
        private readonly IHttpClientFactory _httpClientFactory;

        public event Action<T> DataReceived;

        public APIHandler(IHttpClientFactory httpClientFactory)
        {
            _timer = new Timer(30000);
            _timer.Elapsed += async (_, __) => await RequestData();
            _httpClientFactory = httpClientFactory;
        }

        public async Task Start(string URL)
        {
            _url = URL;
            await RequestData();
            _timer.Start();
        }

        //is called continuously and raises the DataReceived event when payload is ready.
        private async Task RequestData()
        {
            var payload = await RequestOnce(_url);
            DataReceived?.Invoke(payload);
        }

        public async Task<T> RequestOnce(string url)
        {
            if (url is null)
                throw new ArgumentNullException(nameof(url));

            var client = _httpClientFactory.CreateClient();

            var jsonString = await client.GetStringAsync(url);
            jsonString = jsonString.Trim('[', ']');
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public void Dispose()
            => _timer.Dispose();
    }
}
