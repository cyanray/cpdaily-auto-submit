using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
