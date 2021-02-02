using cpdaily_auto_submit.CpdailyModels;
using cpdaily_auto_submit.LoginWorkers;
using System;
using System.Text;
using System.Threading.Tasks;

namespace cpdaily_auto_submit
{
    class Program
    {
        static async Task Main(string[] args)
        {
            DeviceInfo deviceInfo = new DeviceInfo
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

            CpdailyCore cpdaily = new CpdailyCore()
            {
                DeviceInfo = deviceInfo
            };

            //SecretKey key = await cpdaily.GetSecretKey();
            //Console.WriteLine($"guid: {key.Guid}, chk: {key.Chk}, fhk: {key.Fhk}");

            Console.WriteLine(CpdailyCrypto.GetOick());
            Console.WriteLine(CpdailyCrypto.GetOick("test"));


            Console.ReadKey();
            Console.WriteLine("Hello World!");
        }
    }
}
