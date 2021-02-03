using System.Threading.Tasks;

namespace cpdaily_auto_submit.LoginWorkers
{
    public abstract class ILoginWorker
    {
        protected const string WebUserAgent = "Mozilla/5.0 (Linux; Android 5.1.1; vmos Build/LMY48G; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/52.0.2743.100 Mobile Safari/537.36  cpdaily/8.2.16 wisedu/8.2.16";

        /// <summary>
        /// 获取登录需要的参数
        /// </summary>
        /// <param name="username">学号</param>
        /// <param name="password">密码</param>
        /// <param name="idsUrl">统一登陆地址</param>
        /// <returns></returns>
        public abstract Task<LoginParameter> GetLoginParameter(string username, string password, string idsUrl);

        /// <summary>
        /// 针对NOTCLOUD接入的IDS登录，可直接返回Cookies
        /// </summary>
        /// <param name="loginParameter">LoginParameter Object</param>
        /// <returns>用于访问校内应用的 Cookie</returns>
        public abstract Task<string> IdsLogin(LoginParameter loginParameter);

        /// <summary>
        /// 针对通用登录，获取Token
        /// </summary>
        /// <param name="loginParameter">LoginParameter Object</param>
        /// <returns>用于后续登录步骤的Token</returns>
        public abstract Task<string> GetEncrypedToken(LoginParameter loginParameter);
    }
}
