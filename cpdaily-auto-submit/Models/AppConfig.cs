using System;
using System.Collections.Generic;
using System.Text;

namespace cpdaily_auto_submit.Models
{
    public class AppConfig
    {
        public string SchoolName { get; set; } = null;
        public List<User> Users { get; set; } = new List<User>();
    }
}
