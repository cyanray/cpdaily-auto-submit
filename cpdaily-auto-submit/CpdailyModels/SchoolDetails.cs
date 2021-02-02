using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Web;

namespace cpdaily_auto_submit.CpdailyModels
{
    public class SchoolDetails
    {
        public enum SchoolJoinType
        {
            NOTCLOUD, CLOUD, CAS
        }

        /// <summary>
        /// SchoolId
        /// </summary>
        public string Id { get; set; }
        public string AppId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public SchoolJoinType JoinType { get; set; }

        public string IdsUrl { get; set; }

        public string CasLoginUrl { get; set; }

        public string GetLoginUrl()
        {
            var serviceUrl = HttpUtility.UrlEncode("https://mobile.campushoy.com/v6/auth/campus/cas/login");
            return JoinType switch
            {
                SchoolJoinType.CAS => $"{CasLoginUrl}/login?service={serviceUrl}",
                SchoolJoinType.CLOUD => $"{IdsUrl}/login?service={serviceUrl}",
                SchoolJoinType.NOTCLOUD => $"{IdsUrl}/mobile/auth?appId={AppId}",
                _ => null,
            };
        }
    }
}
