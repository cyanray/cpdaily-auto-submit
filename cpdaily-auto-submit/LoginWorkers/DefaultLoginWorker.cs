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
        public const string WebUserAgent = "Mozilla/5.0 (Linux; Android 5.1.1; vmos Build/LMY48G; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/52.0.2743.100 Mobile Safari/537.36  cpdaily/8.2.16 wisedu/8.2.16";

        private Task<bool> NeedCaptcha(Uri loginPageUri)
        {
            // TODO: IMPL
            return Task.FromResult(false);
        }

        public override async Task<string> GetEncrypedToken(string username, string password, string idsUrl)
        {
            CookieContainer CookieContainer = new CookieContainer();
            RestClient LoginClient = new RestClient(idsUrl)
            {
                CookieContainer = CookieContainer
            };
            var request = new RestRequest(Method.POST);
            request.AddHeader("User-Agent", WebUserAgent);
            var response = await LoginClient.ExecutePostAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("非200状态响应");

            var needCaptchaTask = NeedCaptcha(response.ResponseUri);

            //** 解析 pwdDefaultEncryptSalt **//
            var matches = Regex.Matches(response.Content, "var pwdDefaultEncryptSalt *= *\"(.*?)\";");
            if (matches.Count <= 0) throw new Exception("没有匹配到 pwdDefaultEncryptSalt。");
            string pwdDefaultEncryptSalt = null;
            if (matches[0].Success)
            {
                pwdDefaultEncryptSalt = matches[0].Groups[1].Value;
            }

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

            if (await needCaptchaTask) throw new NeedCaptchaException();


            byte[] iv = Encoding.ASCII.GetBytes(CpdailyCrypto.RandomString(16));
            string tPassword = CpdailyCrypto.RandomString(64) + password;
            string encryptedPwd = CpdailyCrypto.AESEncrypt(tPassword, pwdDefaultEncryptSalt, iv);

            string loginUrl = response.ResponseUri.GetLeftPart(UriPartial.Authority) + action;
            do
            {
                LoginClient = new RestClient(loginUrl)
                {
                    CookieContainer = CookieContainer,
                    FollowRedirects = false
                };
                request = new RestRequest(Method.POST);
                request.AddHeader("User-Agent", WebUserAgent);
                request.AddParameter("username", username);
                request.AddParameter("password", encryptedPwd);
                request.AddParameter("captchaResponse", "");
                request.AddParameter("lt", lt);
                request.AddParameter("dllt", "mobileLogin");
                request.AddParameter("execution", execution);
                request.AddParameter("_eventId", "submit");
                request.AddParameter("rmShown", "1");
                response = await LoginClient.ExecutePostAsync(request);

                loginUrl = response.Headers.Where(x => x.Name == "Location").Select(x => x.Value.ToString()).FirstOrDefault();
            } while (!string.IsNullOrEmpty(loginUrl));


            foreach (var cookie in response.Cookies)
            {
                Console.WriteLine($"{cookie.Name}:{cookie.Value};");
            }

            //if (response.StatusCode != HttpStatusCode.OK)
            //    throw new Exception("非200状态响应");

            //matches = Regex.Matches(response.ResponseUri.OriginalString, "mobile_token=(.*)");
            //if (matches.Count > 0)
            //{
            //    return matches[0].Groups[1].Value;
            //}
            return "";
            //// TODO: parse error message.
            //throw new Exception("登录失败。");
        }
    
    }
}
