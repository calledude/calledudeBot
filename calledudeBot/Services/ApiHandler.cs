using Newtonsoft.Json;
using System;
using System.Net;
using System.Timers;

namespace calledudeBot.Services
{
    public enum RequestData
    {
        TwitchUser, OsuUser, OsuSong
    }

    public class APIHandler
    {
        private string URL;
        private RequestData requestedData;
        private WebClient client = new WebClient();
        private Timer timer;
        public event Action<JsonData> DataReceived;

        public APIHandler(string URL, RequestData requestedData, string token = null)
        {
            if (requestedData == RequestData.TwitchUser) client.Headers.Add("Client-ID", token);
            this.requestedData = requestedData;
            this.URL = URL;

            timer = new Timer(30000);
            timer.Elapsed += requestData;
        }

        public void Start()
        {
            timer.Start();
            requestData(null, null);
        }

        private void requestData(object sender, ElapsedEventArgs e)
        {
            string jsonString = client.DownloadString(URL);
            if(requestedData == RequestData.OsuUser)
            {
                jsonString = "{\"osuUserData\":" + jsonString + "}"; //I hate json
            }
            else if(requestedData == RequestData.TwitchUser)
            {
                jsonString = jsonString.Replace("data", "twitchData");
            }
            JsonData jsonData = JsonConvert.DeserializeObject<JsonData>(jsonString);
            DataReceived?.Invoke(jsonData);
        }

        public JsonData requestOnce()
        {
            string jsonString = client.DownloadString(URL);
            if (requestedData == RequestData.OsuSong)
            {
                jsonString = "{\"osuSongData\":" + jsonString + "}"; //I hate json
            }
            return JsonConvert.DeserializeObject<JsonData>(jsonString);
        }
    }
}
