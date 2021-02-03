using System.Collections.Generic;
using System.Net;

namespace cpdaily_auto_submit.LoginWorkers
{
    public class LoginParameter
    {
        public string Username { get; set; } = null;
        public string Password { get; set; } = null;
        public string EncryptedPassword { get; set; } = null;
        public string IdsUrl { get; set; } = null;
        public string ActionUrl { get; set; } = null;
        public bool NeedCaptcha { get; set; } = false;
        public string CaptchaImageUrl { get; set; } = null;
        public string CaptchaValue { get; set; } = null;
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
        public CookieContainer CookieContainer { get; set; } = new CookieContainer();
    }
}
