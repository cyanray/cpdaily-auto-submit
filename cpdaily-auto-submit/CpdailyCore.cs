using cpdaily_auto_submit.CpdailyModels;
using cpdaily_auto_submit.LoginWorkers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace cpdaily_auto_submit
{
    /// <summary>
    /// Cpdaily API
    /// </summary>
    public class CpdailyCore
    {
        /// <summary>
        /// 设备信息
        /// </summary>
        public DeviceInfo DeviceInfo { get; set; } = new DeviceInfo()
        {
            AppVersion = "8.2.16",
            SystemName = "android",
            SystemVersion = "5.1.1",
            DeviceId = "vmosserivmosvmos",
            Model = "vmos",
            Longitude = 0,
            Latitude = 0,
            UserId = ""
        };

        private const string ApiUserAgent = "Mozilla/5.0 (Linux; Android 5.1.1; vmos Build/LMY48G; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/52.0.2743.100 Mobile Safari/537.36 okhttp/3.12.4";
        private const string WebUserAgent = "Mozilla/5.0 (Linux; Android 5.1.1; vmos Build/LMY48G; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/52.0.2743.100 Mobile Safari/537.36  cpdaily/8.2.16 wisedu/8.2.16";
        private const string SaltForGetSecretKey = "2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824";
        private readonly CookieContainer CookieContainer = new CookieContainer();

        /// <summary>
        /// 获取后续加密用的密钥
        /// </summary>
        /// <param name="guid">随机的GUID</param>
        /// <returns>SecretKey</returns>
        public async Task<SecretKey> GetSecretKeyAsync(Guid guid)
        {
            string p = $"{guid}|firstv";
            p = Convert.ToBase64String(CpdailyCrypto.RSAEncrpty(Encoding.ASCII.GetBytes(p), CpdailyCrypto.PublicKey));
            string s = $"p={p}&{SaltForGetSecretKey}";
            s = CpdailyCrypto.MD5(s).ToLower();
            string url = "https://mobile.campushoy.com/app/auth/dynamic/secret/getSecretKey/v-8213";
            RestClient client = new RestClient(url)
            {
                CookieContainer = CookieContainer
            };
            var request = new RestRequest(Method.POST);
            request.AddHeader("sessionTokenKey", "szFn6zAbjjU=");
            request.AddHeader("SessionToken", "szFn6zAbjjU=");
            request.AddHeader("CpdailyClientType", "CPDAILY");
            request.AddHeader("CpdailyStandAlone", "0");
            request.AddHeader("CpdailyInfo", DeviceInfo.EncryptCache);
            request.AddHeader("User-Agent", ApiUserAgent);
            request.AddJsonBody(new { p, s });
            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("非200状态响应");
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
            if (apiResponse.ErrorCode != 0)
            {
                throw new Exception($"出现错误: {apiResponse.ErrorMessage}, 错误代码: {apiResponse.ErrorCode}。");
            }
            string decretedData = Encoding.ASCII.GetString(CpdailyCrypto.RSADecrpty(Convert.FromBase64String(apiResponse.Data), CpdailyCrypto.PrivateKey));
            string[] data = decretedData.Split("|");
            return new SecretKey { Guid = Guid.Parse(data[0]), Chk = data[1], Fhk = data[2] };
        }

        /// <summary>
        /// 获取后续加密用的密钥
        /// </summary>
        /// <returns>SecretKey</returns>
        public Task<SecretKey> GetSecretKeyAsync()
        {
            return GetSecretKeyAsync(Guid.NewGuid());
        }

        /// <summary>
        /// 获取所有入驻的学校
        /// </summary>
        /// <returns>School[]</returns>
        public async Task<School[]> GetSchoolsAsync()
        {
            string url = $"https://static.campushoy.com/apicache/tenantListSort?v={DateTimeOffset.Now.ToUnixTimeMilliseconds()}&oick={CpdailyCrypto.GetOick()}";
            RestClient client = new RestClient(url)
            {
                CookieContainer = CookieContainer
            };
            var request = new RestRequest(Method.GET);
            request.AddHeader("sessionTokenKey", "szFn6zAbjjU=");
            request.AddHeader("SessionToken", "szFn6zAbjjU=");
            request.AddHeader("CpdailyClientType", "CPDAILY");
            request.AddHeader("CpdailyStandAlone", "0");
            request.AddHeader("CpdailyInfo", DeviceInfo.EncryptCache);
            request.AddHeader("User-Agent", ApiUserAgent);
            var response = await client.ExecuteGetAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("非200状态响应");
            var json = JObject.Parse(response.Content);
            if (json["errCode"].Value<int>() != 0)
            {
                throw new Exception($"出现错误: {json["errMsg"].Value<string>()}, 错误代码: {json["errCode"].Value<int>()}。");
            }
            List<School> schools = new List<School>();
            var lists = JArray.FromObject(json["data"]);
            foreach (JObject t in lists)
            {
                var list = JArray.FromObject(t["datas"]);
                schools.AddRange(list.ToObject<List<School>>());
            }
            return schools.ToArray();
        }

        /// <summary>
        /// 获取学校的详细信息(用于登录或其他)
        /// </summary>
        /// <param name="schoolId">学校ID</param>
        /// <param name="chk">通过GetSecretKeyAsync()获取到的Chk密钥</param>
        /// <returns>SchoolDetails</returns>
        public async Task<SchoolDetails> GetSchoolDetailsAsync(string schoolId, string chk)
        {
            var encryptedId = CpdailyCrypto.DESEncrypt(schoolId, CpdailyCrypto.GetDESKey(chk), CpdailyCrypto.IV);
            string url = $"https://mobile.campushoy.com/v6/config/guest/tenant/info/v-8213";
            RestClient client = new RestClient(url)
            {
                CookieContainer = CookieContainer
            };
            var request = new RestRequest(Method.GET);
            request.AddHeader("sessionTokenKey", "szFn6zAbjjU=");
            request.AddHeader("SessionToken", "szFn6zAbjjU=");
            request.AddHeader("CpdailyClientType", "CPDAILY");
            request.AddHeader("CpdailyStandAlone", "0");
            request.AddHeader("CpdailyInfo", DeviceInfo.EncryptCache);
            request.AddHeader("User-Agent", ApiUserAgent);
            request.AddParameter("a", encryptedId);
            request.AddParameter("b", "firstv");
            request.AddParameter("oick", CpdailyCrypto.GetOick());
            var response = await client.ExecuteGetAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("非200状态响应");
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
            if (apiResponse.ErrorCode != 0)
            {
                throw new Exception($"出现错误: {apiResponse.ErrorMessage}, 错误代码: {apiResponse.ErrorCode}。");
            }
            string decretedData = CpdailyCrypto.DESDecrypt(apiResponse.Data, CpdailyCrypto.GetDESKey(chk), CpdailyCrypto.IV);
            return JArray.Parse(decretedData)[0].ToObject<SchoolDetails>();
        }

        /// <summary>
        /// 获取学校的详细信息(用于登录或其他)
        /// </summary>
        /// <param name="school">School Object</param>
        /// <param name="secretKey">SecretKey Object</param>
        /// <returns>SchoolDetails</returns>
        public Task<SchoolDetails> GetSchoolDetailsAsync(School school, SecretKey secretKey)
        {
            return GetSchoolDetailsAsync(school.Id, secretKey.Chk);
        }

        /// <summary>
        /// 通用登录，今日校园的Auth阶段(未完成)
        /// </summary>
        /// <param name="encryptedToken">通过LoginWorker得到的Token</param>
        /// <param name="schoolId">School Id</param>
        /// <param name="chk">通过GetSecretKeyAsync()获取到的Chk密钥</param>
        /// <returns>CookieContainer(以后将会修改)</returns>
        public async Task<CookieContainer> CpdailyAuth(string encryptedToken, string schoolId, string chk)
        {
            string encryptedData = JsonConvert.SerializeObject(new { c = schoolId, d = encryptedToken });
            encryptedData = CpdailyCrypto.DESEncrypt(encryptedData, CpdailyCrypto.GetDESKey(chk), CpdailyCrypto.IV);
            var result = new CookieContainer();
            string url = "https://mobile.campushoy.com/app/auth/authentication/notcloud/login/v-8213";
            RestClient client = new RestClient(url)
            {
                CookieContainer = result
            };
            var request = new RestRequest(Method.POST);
            request.AddHeader("sessionTokenKey", "szFn6zAbjjU=");
            request.AddHeader("SessionToken", "szFn6zAbjjU=");
            request.AddHeader("clientType", "cpdaily_student");
            request.AddHeader("deviceType", "1");
            request.AddHeader("CpdailyClientType", "CPDAILY");
            request.AddHeader("CpdailyStandAlone", "0");
            request.AddHeader("CpdailyInfo", DeviceInfo.EncryptCache);
            request.AddHeader("User-Agent", ApiUserAgent);
            request.AddJsonBody(new { a = encryptedData, b = "firstv" });
            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("非200状态响应");
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
            if (apiResponse.ErrorCode != 0)
            {
                throw new Exception($"出现错误: {apiResponse.ErrorMessage}, 错误代码: {apiResponse.ErrorCode}。");
            }
            var json = JObject.Parse(CpdailyCrypto.DESDecrypt(apiResponse.Data, CpdailyCrypto.GetDESKey(chk), CpdailyCrypto.IV));
            string tgc = json["tgc"].Value<string>();
            if (string.IsNullOrEmpty(tgc))   // TODO: 
            {
                string mobile = json["mobile"].Value<string>();
                await SendVerifyMessage(result, mobile);
                string code = Console.ReadLine();
                await VerifyCode(result, encryptedToken, mobile, code);
            }
            return result;
        }

        /// <summary>
        /// 通用登录，设备异常需验证，发送验证码(未完成)
        /// </summary>
        /// <param name="cookieContainer">通过CpdailyAuth()得到的CookieContainer</param>
        /// <param name="phoneNumber">手机号(需要修改:可在Auth得到)</param>
        /// <returns></returns>
        public async Task SendVerifyMessage(CookieContainer cookieContainer, string phoneNumber)
        {
            string mobile = CpdailyCrypto.DESEncrypt(phoneNumber, "QTZ&A@54", CpdailyCrypto.IV);
            string url = "https://mobile.campushoy.com/v6/auth/deviceChange/mobile/messageCode/v2";
            RestClient client = new RestClient(url)
            {
                CookieContainer = cookieContainer
            };
            var request = new RestRequest(Method.POST);
            request.AddHeader("sessionTokenKey", "szFn6zAbjjU=");
            request.AddHeader("SessionToken", "szFn6zAbjjU=");
            request.AddHeader("clientType", "cpdaily_student");
            request.AddHeader("deviceType", "1");
            request.AddHeader("CpdailyClientType", "CPDAILY");
            request.AddHeader("CpdailyStandAlone", "0");
            request.AddHeader("CpdailyInfo", DeviceInfo.EncryptCache);
            request.AddHeader("User-Agent", ApiUserAgent);
            request.AddJsonBody(new { mobile });
            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("非200状态响应");
            //var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
            //if (apiResponse.ErrorCode != 0)
            //{
            //    throw new Exception($"出现错误: {apiResponse.ErrorMessage}, 错误代码: {apiResponse.ErrorCode}。");
            //}

        }

        /// <summary>
        /// 通用登录，设备异常需验证，提交验证码(未完成)
        /// </summary>
        /// <param name="cookieContainer">通过CpdailyAuth()得到的CookieContainer</param>
        /// <param name="encryptedToken">通过LoginWorker得到的Token</param>
        /// <param name="phoneNumber">手机号</param>
        /// <param name="messageCode">验证码</param>
        /// <returns></returns>
        public async Task VerifyCode(CookieContainer cookieContainer, string encryptedToken, string phoneNumber, string messageCode)
        {
            string url = "https://mobile.campushoy.com/v6/auth/deviceChange/validateMessageCode";
            RestClient client = new RestClient(url)
            {
                CookieContainer = cookieContainer
            };
            var request = new RestRequest(Method.POST);
            request.AddHeader("sessionTokenKey", "szFn6zAbjjU=");
            request.AddHeader("SessionToken", "szFn6zAbjjU=");
            request.AddHeader("clientType", "cpdaily_student");
            request.AddHeader("deviceType", "1");
            request.AddHeader("CpdailyClientType", "CPDAILY");
            request.AddHeader("CpdailyStandAlone", "0");
            request.AddHeader("CpdailyInfo", DeviceInfo.EncryptCache);
            request.AddHeader("User-Agent", ApiUserAgent);
            request.AddJsonBody(new { ticket = encryptedToken, mobile = phoneNumber, messageCode });
            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("非200状态响应");
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
            if (apiResponse.ErrorCode != 0)
            {
                throw new Exception($"出现错误: {apiResponse.ErrorMessage}, 错误代码: {apiResponse.ErrorCode}。");
            }

        }

        /// <summary>
        /// 手机验证码登录(发送验证码)
        /// </summary>
        /// <param name="phoneNumber">手机号</param>
        /// <param name="chk">通过GetSecretKeyAsync()获取到的Chk密钥</param>
        /// <returns>void</returns>
        public async Task MobileLoginAsync(string phoneNumber, string chk)
        {
            string mobile = CpdailyCrypto.DESEncrypt(phoneNumber, CpdailyCrypto.GetDESKey(chk), CpdailyCrypto.IV);
            string url = "https://mobile.campushoy.com/app/auth/authentication/mobile/messageCode/v-8213";
            RestClient client = new RestClient(url)
            {
                CookieContainer = CookieContainer
            };
            var request = new RestRequest(Method.POST);
            request.AddHeader("sessionTokenKey", "szFn6zAbjjU=");
            request.AddHeader("SessionToken", "szFn6zAbjjU=");
            request.AddHeader("clientType", "cpdaily_student");
            request.AddHeader("deviceType", "1");
            request.AddHeader("CpdailyClientType", "CPDAILY");
            request.AddHeader("CpdailyStandAlone", "0");
            request.AddHeader("CpdailyInfo", DeviceInfo.EncryptCache);
            request.AddHeader("User-Agent", ApiUserAgent);
            request.AddJsonBody(new { a = mobile, b = "firstv" });
            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("非200状态响应");
            // TODO: parse error message.
        }

        /// <summary>
        /// 手机验证码登录(提交验证码)
        /// </summary>
        /// <param name="phoneNumber">手机号码</param>
        /// <param name="code">验证码</param>
        /// <param name="chk">通过GetSecretKeyAsync()获取到的Chk密钥</param>
        /// <returns>LoginResult</returns>
        public async Task<LoginResult> MobileLoginAsync(string phoneNumber, string code, string chk)
        {
            string data = CpdailyCrypto.DESEncrypt(
                JsonConvert.SerializeObject(new { d = code, c = phoneNumber }),
                CpdailyCrypto.GetDESKey(chk),
                CpdailyCrypto.IV);
            string url = "https://mobile.campushoy.com/app/auth/authentication/mobileLogin/v-8213";
            RestClient client = new RestClient(url)
            {
                CookieContainer = CookieContainer
            };
            var request = new RestRequest(Method.POST);
            request.AddHeader("sessionTokenKey", "szFn6zAbjjU=");
            request.AddHeader("SessionToken", "szFn6zAbjjU=");
            request.AddHeader("clientType", "cpdaily_student");
            request.AddHeader("deviceType", "1");
            request.AddHeader("CpdailyClientType", "CPDAILY");
            request.AddHeader("CpdailyStandAlone", "0");
            request.AddHeader("CpdailyInfo", DeviceInfo.EncryptCache);
            request.AddHeader("User-Agent", ApiUserAgent);
            request.AddJsonBody(new { a = data, b = "firstv" });
            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("非200状态响应");
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
            if (apiResponse.ErrorCode != 0)
            {
                throw new Exception($"出现错误: {apiResponse.ErrorMessage}, 错误代码: {apiResponse.ErrorCode}。");
            }
            var json = JObject.Parse(CpdailyCrypto.DESDecrypt(apiResponse.Data, CpdailyCrypto.GetDESKey(chk), CpdailyCrypto.IV));
            return json.ToObject<LoginResult>();
        }

        /// <summary>
        /// 获取用于访问校内应用的 Cookie
        /// </summary>
        /// <param name="loginResult">LoginResult Object</param>
        /// <returns>Cookie string</returns>
        public async Task<string> UserStoreAppListAsync(LoginResult loginResult)
        {
            string sessionToken = CpdailyCrypto.DESEncrypt(
                loginResult.SessionToken,
                "XCE927==",
                CpdailyCrypto.IV);
            string tgc = CpdailyCrypto.DESEncrypt(
                loginResult.Tgc,
                "XCE927==",
                CpdailyCrypto.IV);

            var amp = new
            {
                AMP1 = new[]
                {
                    new {name="sessionToken", value=loginResult.SessionToken},
                    new {name="sessionToken", value=loginResult.SessionToken}
                },
                AMP2 = new[]
                {
                    new {name="CASTGC", value=loginResult.Tgc},
                    new {name="AUTHTGC", value=loginResult.Tgc},
                    new {name="sessionToken", value=loginResult.SessionToken},
                    new {name="sessionToken", value=loginResult.SessionToken}
                }
            };

            string ampCookies = CpdailyCrypto.DESEncrypt(
                JsonConvert.SerializeObject(amp),
                "XCE927==",
                CpdailyCrypto.IV);

            string url = $"https://cqjtu.campusphere.net/wec-portal-mobile/client/userStoreAppList?oick={CpdailyCrypto.GetOick(loginResult.UserId)}";
            IRestResponse response = null;
            // RestSharp 不会处理复杂的跳转，因此自行循环处理
            do
            {
                RestClient client = new RestClient(url)
                {
                    CookieContainer = CookieContainer,
                    FollowRedirects = false
                };
                var request = new RestRequest(Method.GET);
                request.AddHeader("sessionTokenKey", sessionToken);
                request.AddHeader("SessionToken", sessionToken);
                request.AddHeader("clientType", "cpdaily_student");
                request.AddHeader("deviceType", "1");
                request.AddHeader("CpdailyClientType", "CPDAILY");
                request.AddHeader("CpdailyStandAlone", "0");
                request.AddHeader("CpdailyInfo", DeviceInfo.EncryptCache);
                request.AddHeader("User-Agent", ApiUserAgent);
                request.AddHeader("TGC", tgc);
                request.AddHeader("AmpCookies", ampCookies);
                request.AddHeader("Cookie", $"CASTGC={loginResult.Tgc}; AUTHTGC={loginResult.Tgc};");
                response = await client.ExecuteGetAsync(request);

                url = response.Headers.Where(x => x.Name == "Location").Select(x => x.Value.ToString()).FirstOrDefault();
            } while (!string.IsNullOrEmpty(url));
            StringBuilder sb = new StringBuilder();
            foreach (var cookie in response.Cookies)
            {
                sb.Append($"{cookie.Name}={cookie.Value}; ");
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取未完成的表单项目
        /// </summary>
        /// <param name="cookies">用于访问校内应用的 Cookie</param>
        /// <returns>FormItem[]</returns>
        public async Task<FormItem[]> GetFormItemsAsync(string cookies)
        {
            string url = "https://cqjtu.campusphere.net/wec-counselor-collector-apps/stu/collector/queryCollectorProcessingList";
            RestClient client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Cookie", cookies);
            request.AddHeader("User-Agent", ApiUserAgent);
            request.AddJsonBody(new { pageNumber = 1, pageSize = 20 });
            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("非200状态响应");
            var json = JObject.Parse(response.Content);
            // TODO: parse error message
            var list = JArray.FromObject(json["datas"]["rows"]);
            return list.ToObject<FormItem[]>();
        }

        /// <summary>
        /// 获取表单的字段
        /// </summary>
        /// <param name="cookies">用于访问校内应用的 Cookie</param>
        /// <param name="wid">FormItem.Wid</param>
        /// <param name="formWid">FormItem.FormWid</param>
        /// <returns>FormField[]</returns>
        public async Task<FormField[]> GetFormFieldsAsync(string cookies, string wid, string formWid)
        {
            string url = "https://cqjtu.campusphere.net/wec-counselor-collector-apps/stu/collector/getFormFields";
            RestClient client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Cookie", cookies);
            request.AddHeader("User-Agent", ApiUserAgent);
            request.AddJsonBody(new {   pageNumber = 1,
                                        pageSize = 20,
                                        formWid = formWid,
                                        collectorWid = wid });
            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("非200状态响应");
            var json = JObject.Parse(response.Content);
            // TODO: parse error message
            var list = JArray.FromObject(json["datas"]["rows"]);
            return list.ToObject<FormField[]>();
        }
    }
}
