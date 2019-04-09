using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Timers;

namespace calledudeBot.Services
{
    public sealed class APIHandler<T> : IDisposable
    {
        private readonly string _url;
        private readonly WebClient _client;
        private readonly Timer _timer;
        public event Action<T> DataReceived;

        public APIHandler(string URL)
        {
            _url = URL;
            _client = new WebClient();
            _timer = new Timer(30000);
            _timer.Elapsed += async (_, __) => await RequestData();
        }

        public async Task Start()
        {
            await RequestData();
            _timer.Start();
        }

        //is called continuously and raises the DataReceived event when payload is ready.
        private async Task RequestData()
        {
            var payload = await RequestOnce();
            DataReceived?.Invoke(payload);
        }

        public async Task<T> RequestOnce()
        {
            var jsonString = await _client.DownloadStringTaskAsync(_url);
            jsonString = jsonString.Trim('[', ']');
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public void Dispose()
        {
            _client.Dispose();
            _timer.Dispose();
        }
    }
}
