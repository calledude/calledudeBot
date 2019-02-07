using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Timers;

namespace calledudeBot.Services
{
    public sealed class APIHandler<T> : IDisposable
    {
        private readonly string URL;
        private readonly WebClient client;
        private readonly Timer timer;
        public event Action<T> DataReceived;

        public APIHandler(string URL)
        {
            this.URL = URL;
            client = new WebClient();
            timer = new Timer(30000);
            timer.Elapsed += async (_,__) => await requestData();
        }

        public async Task Start()
        {
            await requestData();
            timer.Start();
        }

        //is called continuously and raises the DataReceived event when payload is ready.
        private async Task requestData()
        {
            var payload = await RequestOnce();
            DataReceived?.Invoke(payload);
        }

        public async Task<T> RequestOnce()
        {
            var jsonString = await client.DownloadStringTaskAsync(URL);
            jsonString = jsonString.Trim('[', ']');
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public void Dispose()
        {
            client.Dispose();
            timer.Dispose();
        }
    }
}
