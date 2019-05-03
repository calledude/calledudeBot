using Newtonsoft.Json;

namespace calledudeBot.Models
{
    public sealed class OsuSong : BaseModel
    {
        [JsonProperty("version")]
        public string BeatmapVersion { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
