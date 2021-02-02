using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace cpdaily_auto_submit.CpdailyModels
{
    public class FormField
    {
        [JsonProperty("wid")]
        public string Wid { get; set; }

        [JsonProperty("formWid")]
        public string FormWid { get; set; }

        [JsonProperty("fieldType")]
        public int FieldType { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("minLength")]
        public int? MinLength { get; set; }

        [JsonProperty("sort")]
        public string Sort { get; set; }

        [JsonProperty("maxLength")]
        public int? MaxLength { get; set; }

        [JsonProperty("isRequired")]
        public int IsRequired { get; set; }

        [JsonProperty("imageCount")]
        public int? ImageCount { get; set; }

        [JsonProperty("hasOtherItems")]
        public int? HasOtherItems { get; set; }

        [JsonProperty("colName")]
        public string ColName { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("minValue")]
        public double? MinValue { get; set; }

        [JsonProperty("maxValue")]
        public double? MaxValue { get; set; }

        [JsonProperty("isDecimal")]
        public bool? IsDecimal { get; set; }

        [JsonProperty("fieldItems")]
        public List<FieldItem> FieldItems { get; set; }
    }
}
