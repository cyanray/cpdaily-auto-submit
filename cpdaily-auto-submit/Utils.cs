using cpdaily_auto_submit.LoginWorkers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Reflection;

namespace cpdaily_auto_submit
{
    internal class Utils
    {
        public static DefaultContractResolver contractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        };

        public static JsonSerializerSettings GlobalSetting = new JsonSerializerSettings
        {
            ContractResolver = contractResolver
        };

        public static Type GetLoginWorkerByName(string name)
        {
            var loginWorkerTypes = 
                Assembly.GetAssembly(typeof(ILoginWorker)).GetTypes()
                .Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(ILoginWorker)))
                .ToList();

            foreach (var type in loginWorkerTypes)
            {
                if (type.GetCustomAttributes(typeof(SchoolNameAttribute), false)
                        .FirstOrDefault() is SchoolNameAttribute schoolNameAttribute)
                {
                    if (schoolNameAttribute.Name == name)
                    {
                        return type;
                    }
                }
            }
            return null;
        }

    }
}
