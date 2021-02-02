using cpdaily_auto_submit.CpdailyModels;
using HtmlAgilityPack;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace cpdaily_auto_submit.LoginWorkers
{
    public class DefaultLoginWorker : ILoginWorker
    {
        private Task<bool> NeedCaptcha(string loginPageUri)
        {
            // TODO: IMPL
            return Task.FromResult(false);
        }

        public override async Task<LoginParameter> GetLoginParameter(string username, string password, string idsUrl)
        {
            LoginParameter loginParameter = new LoginParameter()
            {
                Username = username,
                Password = password,
                IdsUrl = idsUrl,
                NeedCaptcha = false
            };
            RestClient LoginClient = new RestClient(idsUrl)
            {
                CookieContainer = loginParameter.CookieContainer
            };
            var request = new RestRequest(Method.POST);
            request.AddHeader("User-Agent", WebUserAgent);
            var response = await LoginClient.ExecutePostAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("非200状态响应");
            string urlRoot = response.ResponseUri.GetLeftPart(UriPartial.Authority);

            var needCaptchaTask = NeedCaptcha(urlRoot);
            loginParameter.CaptchaImageUrl = $"{urlRoot}/authserver/captcha.html";

            //** 解析 pwdDefaultEncryptSalt **//
            var matches = Regex.Matches(response.Content, "var pwdDefaultEncryptSalt *= *\"(.*?)\";");
            if (matches.Count <= 0) throw new Exception("没有匹配到 pwdDefaultEncryptSalt。");
            string pwdDefaultEncryptSalt = null;
            if (matches[0].Success)
            {
                pwdDefaultEncryptSalt = matches[0].Groups[1].Value;
            }

            // update encryptedPwd
            byte[] iv = Encoding.ASCII.GetBytes(CpdailyCrypto.RandomString(16));
            string tPassword = CpdailyCrypto.RandomString(64) + password;
            string encryptedPwd = CpdailyCrypto.AESEncrypt(tPassword, pwdDefaultEncryptSalt, iv);
            loginParameter.EncryptedPassword = encryptedPwd;

            //** 解析 action、lt、execution **//
            var doc = new HtmlDocument();
            doc.LoadHtml(response.Content);
            string action = doc.DocumentNode
                .SelectNodes(@"//*[@id=""casLoginForm""]")[0]
                .GetAttributeValue("action", string.Empty);
            string lt = doc.DocumentNode
                .SelectNodes(@"//*[@id=""casLoginForm""]/input[1]")[0]
                .GetAttributeValue("value", string.Empty);
            string execution = doc.DocumentNode
                .SelectNodes(@"//*[@id=""casLoginForm""]/input[3]")[0]
                .GetAttributeValue("value", string.Empty);

            loginParameter.Parameters.Add("lt", lt);
            loginParameter.Parameters.Add("execution", execution);
            loginParameter.ActionUrl = urlRoot + action;

            loginParameter.NeedCaptcha = await needCaptchaTask;

            return loginParameter;
        }

        public override async Task<string> IdsLogin(LoginParameter loginParameter)
        {
            string loginUrl = loginParameter.ActionUrl;
            IRestResponse response = null;
            do
            {
                RestClient client = new RestClient(loginUrl)
                {
                    CookieContainer = loginParameter.CookieContainer,
                    FollowRedirects = false
                };
                var request = new RestRequest(Method.POST);
                request.AddHeader("User-Agent", WebUserAgent);
                request.AddParameter("username", loginParameter.Username);
                request.AddParameter("password", loginParameter.EncryptedPassword);
                request.AddParameter("captchaResponse", loginParameter.CaptchaValue);
                request.AddParameter("lt", loginParameter.Parameters["lt"]);
                request.AddParameter("dllt", "mobileLogin");
                request.AddParameter("execution", loginParameter.Parameters["execution"]);
                request.AddParameter("_eventId", "submit");
                request.AddParameter("rmShown", "1");
                response = await client.ExecutePostAsync(request);

                loginUrl = response.Headers.Where(x => x.Name == "Location").Select(x => x.Value.ToString()).FirstOrDefault();
            } while (!string.IsNullOrEmpty(loginUrl));

            StringBuilder sb = new StringBuilder();
            foreach (var cookie in response.Cookies)
            {
                sb.Append($"{cookie.Name}={cookie.Value}; ");
            }
            return sb.ToString();
        }

        public override Task<string> GetEncrypedToken(LoginParameter loginParameter)
        {
            throw new NotImplementedException();
        }
    }
}
