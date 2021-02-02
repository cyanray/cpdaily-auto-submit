using cpdaily_auto_submit.CpdailyModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace cpdaily_auto_submit.LoginWorkers
{
    public abstract class ILoginWorker
    {
        public abstract Task<string> GetEncrypedToken(string username, string password, string idsUrl);
    }
}
