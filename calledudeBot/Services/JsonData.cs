using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace calledudeBot.Services
{
    public class JsonData
    {
        //osu
        public List<OsuData> osuData { get; set; }

        //Twitch
        public List<Data> data { get; set; }

    }
    public class OsuData
    {
        public string user_id { get; set; }
        public string username { get; set; }
        public string playcount { get; set; }
        public string pp_rank { get; set; }
        public string level { get; set; }
        public string pp_raw { get; set; }
        public string accuracy { get; set; }
        public string pp_country_rank { get; set; }
        public List<object> events { get; set; }
    }
    //Twitch related data
    public class Data
    {
        public string title { get; set; }
        public DateTime started_at { get; set; }
    }
    
}


