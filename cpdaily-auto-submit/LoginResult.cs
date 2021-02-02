using cpdaily_auto_submit.CpdailyModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace cpdaily_auto_submit
{
    public class LoginResult
    {
        public string Token { get; set; }
        public string EncryptedToken { get; set; }
        public CookieContainer CpdailyCookies { get; set; }
        public SchoolDetails SchoolDetails { get; set; }
        //****new****//
        public string AuthId { get; set; }
        public string DeviceStatus { get; set; }
        [JsonProperty("deviceExceptionMsg")]
        public string DeviceExceptionMessage { get; set; }
        public string Name { get; set; }
        public string OpenId { get; set; }
        public string PersonId { get; set; }
        public string SessionToken { get; set; }
        public string TenantId { get; set; }
        public string Tgc { get; set; }
        public string UserId { get; set; }
    }
}
