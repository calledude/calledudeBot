using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
        public event Func<JsonData, Task> DataReceived;

        public APIHandler(string URL, Caller caller, string token = null)
        {
            if (caller == Caller.Discord) client.Headers.Add("Client-ID", token);

            this.caller = caller;
            this.URL = URL;
            var timer = new Timer(30000);
            timer.Elapsed += requestData;
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Start();
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
