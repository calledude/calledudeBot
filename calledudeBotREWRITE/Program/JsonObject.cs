using System;
using System.Collections.Generic;

namespace calledudeBot
{
    public class Data
    {
        public string id { get; set; }
        public string user_id { get; set; }
        public string game_id { get; set; }
        public List<object> community_ids { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public string viewer_count { get; set; }
        public DateTime started_at { get; set; }
        public string language { get; set; }
        public string thumbnail_url { get; set; }
    }

    public class Pagination
    {
        public string cursor { get; set; }
    }

    public class TwitchData
    {
        public List<Data> data { get; set; }
        public Pagination pagination { get; set; }
    }
}
