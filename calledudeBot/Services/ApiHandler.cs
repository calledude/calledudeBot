using Newtonsoft.Json;
using System;
using System.Net;
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
            timer.Elapsed += requestData;
        }

        public void Start()
        {
            requestData(null, null);
            timer.Start();
        }

        //is called continuously and raises the DataReceived event when payload is ready.
        private void requestData(object sender, ElapsedEventArgs e)
        {
            var payload = RequestOnce();
            DataReceived?.Invoke(payload);
        }

        public T RequestOnce()
        {
            string jsonString = client.DownloadString(URL).Trim('[', ']');
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public void Dispose()
        {
            client.Dispose();
            timer.Dispose();
        }
    }
}
