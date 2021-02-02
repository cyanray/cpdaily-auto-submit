using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace cpdaily_auto_submit.CpdailyModels
{
    internal class ApiResponse
    {
        [JsonProperty("errCode")]
        public int ErrorCode { get; set; }
        [JsonProperty("errMsg")]
        public string ErrorMessage { get; set; }
        [JsonProperty("data")]
        public string Data { get; set; }
    }
}
