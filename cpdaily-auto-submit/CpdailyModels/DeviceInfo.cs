using Newtonsoft.Json;

namespace cpdaily_auto_submit.CpdailyModels
{
    public class DeviceInfo
    {
        public string SystemName { get; set; }
        public string SystemVersion { get; set; }
        public string Model { get; set; }
        public string DeviceId { get; set; }
        public string AppVersion { get; set; }
        [JsonProperty("lon")]
        public double Longitude { get; set; }
        [JsonProperty("lat")]
        public double Latitude { get; set; }
        public string UserId { get; set; }

        [JsonIgnore]
        private string encryptCache = null;
        [JsonIgnore]
        public string EncryptCache
        {
            get
            {
                if (string.IsNullOrEmpty(encryptCache))
                {
                    encryptCache = Encrypt();
                }
                return encryptCache;
            }
        }

        public string Encrypt()
        {
            string json = JsonConvert.SerializeObject(this, JsonUtils.GlobalSetting);
            return CpdailyCrypto.DESEncrypt(json, "ST83=@XV", CpdailyCrypto.IV);
        }
    }
}
