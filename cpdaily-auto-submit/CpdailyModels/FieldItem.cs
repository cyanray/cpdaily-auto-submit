using Newtonsoft.Json;

namespace cpdaily_auto_submit.CpdailyModels
{
    public class FieldItem
    {
        [JsonProperty("itemWid")]
        public string ItemWid { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("isOtherItems")]
        public bool? IsOtherItems { get; set; }

        [JsonProperty("contendExtend")]
        public string ContendExtend { get; set; }

        [JsonProperty("isSelected")]
        public bool? IsSelected { get; set; }
    }
}
