using Newtonsoft.Json;

namespace cpdaily_auto_submit.CpdailyModels
{
    public class School
    {
        public string Id { get; set; }
        public string Name { get; set; }
        [JsonProperty("img")]
        public string ImageUrl { get; set; }
    }
}
