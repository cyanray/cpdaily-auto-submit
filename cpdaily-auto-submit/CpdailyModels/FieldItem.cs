﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace cpdaily_auto_submit.CpdailyModels
{
    public class FieldItem
    {
        [JsonProperty("itemWid")]
        public string ItemWid { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("isOtherItems")]
        public int IsOtherItems { get; set; }

        [JsonProperty("contendExtend")]
        public string ContendExtend { get; set; }

        [JsonProperty("isSelected")]
        public object IsSelected { get; set; }
    }
}