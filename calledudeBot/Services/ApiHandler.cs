using Newtonsoft.Json;
using System;
using System.Net;
using System.Timers;

namespace calledudeBot.Services
{
    public enum Caller
    {
        Discord, Twitch
    }
    public class APIHandler
    {
        private string URL;
        private Caller caller;
        private WebClient client = new WebClient();
        public event Action<JsonData> DataReceived;

        public APIHandler(string URL, Caller caller, string token = null)
        {
            if (caller == Caller.Discord) client.Headers.Add("Client-ID", token);

            this.caller = caller;
            this.URL = URL;
        }

        public void Start()
        {
            var timer = new System.Timers.Timer(30000);
            timer.Elapsed += requestData;
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Start();

            requestData(null, null);
        }

        private void requestData(object sender, ElapsedEventArgs e)
        {
            string jsonString = client.DownloadString(URL);
            if(caller == Caller.Twitch)
            {
                jsonString = "{\"osuData\":" + jsonString + "}"; //I hate json
             }
            JsonData jsonData = JsonConvert.DeserializeObject<JsonData>(jsonString);
            DataReceived?.Invoke(jsonData);
        }
    }
}
