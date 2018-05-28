using System;
using System.Collections.Generic;

namespace calledudeBot.Services
{
    public class JsonData
    {
        //osu
        public List<OsuData> osuData { get; set; }

        //Twitch
        public List<Data> data { get; set; }

        //osuSongData
        public List<OsuSongData> osuSongData { get; set; }

    }
    public class OsuData
    {
        public string user_id { get; set; }
        public string username { get; set; }
        public int pp_rank { get; set; }
        public string level { get; set; }
        public float pp_raw { get; set; }
        public float accuracy { get; set; }
        public int pp_country_rank { get; set; }
        public List<object> events { get; set; }
    }

    public class OsuSongData
    {
        public string version { get; set; }
        public string artist { get; set; }
        public string title { get; set; }
    }

    //Twitch related data
    public class Data
    {
        public string title { get; set; }
        public DateTime started_at { get; set; }
    }
    
}


