using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace cpdaily_auto_submit
{
    internal class JsonUtils
    {
        public static DefaultContractResolver contractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        };

        public static JsonSerializerSettings GlobalSetting = new JsonSerializerSettings
        {
            ContractResolver = contractResolver
        };

    }
}
