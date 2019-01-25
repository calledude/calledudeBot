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

    public class APIHandler : IDisposable
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

        //is called continuously and raises the DataReceived event when payload is ready.
        private void requestData(object sender, ElapsedEventArgs e)
        {
            JsonData jsonData = requestOnce();
            DataReceived?.Invoke(jsonData);
        }

        public JsonData requestOnce()
        {
            string jsonString = client.DownloadString(URL);
            jsonString = formatJsonString(jsonString);
            return JsonConvert.DeserializeObject<JsonData>(jsonString);
        }

        private string formatJsonString(string jsonString)
        {
            if (requestedData == RequestData.OsuUser)
            {
                jsonString = "{\"osuUserData\":" + jsonString + "}"; //I hate json
            }
            else if (requestedData == RequestData.TwitchUser)
            {
                jsonString = jsonString.Replace("data", "twitchData");
            }
            else if (requestedData == RequestData.OsuSong)
            {
                jsonString = "{\"osuSongData\":" + jsonString + "}"; //I hate json
            }
            return jsonString;
        }

        public void Dispose()
        {
            client.Dispose();
            timer.Dispose();
        }
    }
}
