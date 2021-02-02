using System;
using System.Collections.Generic;
using System.Text;

namespace cpdaily_auto_submit.LoginWorkers
{
    public class NeedCaptchaException : Exception
    {
        public NeedCaptchaException()
        {
        }

        public NeedCaptchaException(string message) : base(message)
        {
        }

        public NeedCaptchaException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
