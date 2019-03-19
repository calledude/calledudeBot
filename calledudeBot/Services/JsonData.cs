using Newtonsoft.Json;
using System.Collections.Generic;

namespace calledudeBot.Services
{
    public sealed class OsuUser
    {
        [JsonProperty("user_id")]
        public string UserID { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("pp_rank")]
        public int Rank { get; set; }

        [JsonProperty("level")]
        public string Level { get; set; }

        [JsonProperty("pp_raw")]
        public float PP { get; set; }

        [JsonProperty("accuracy")]
        public float Accuracy { get; set; }

        [JsonProperty("pp_country_rank")]
        public int CountryRank { get; set; }

        [JsonProperty("events")]
        public List<object> Events { get; set; }
    }

    public sealed class OsuSong
    {
        [JsonProperty("version")]
        public string BeatmapVersion { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}


