using cpdaily_auto_submit.CpdailyModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace cpdaily_auto_submit.Models
{
    public class AppConfig
    {
        public string SchoolName { get; set; } = null;
        public string Address { get; set; } = "";
        public double Longitude { get; set; } = 0;
        public double Latitude { get; set; } = 0;
        public List<User> Users { get; set; } = new List<User>();
        public List<FormFieldChange> FormFields { get; set; } = new List<FormFieldChange>();
    }
}
