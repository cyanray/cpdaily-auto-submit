using cpdaily_auto_submit.LoginWorkers;
using McMaster.Extensions.CommandLineUtils;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace cpdaily_auto_submit
{
    [Command(Description = "登录并初始化签到需要的数据")]
    class InitCommand : CommandBase
    {
        [Option("-u|--username", Description = "学号")]
        [Required]
        public string Username { get; set; }

        [Option("-p|--password", Description = "密码")]
        [Required]
        public string Password { get; set; }

        [Option("-s|--school", Description = "学校名称")]
        [Required]
        public string SchoolName { get; set; }

        protected async override Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            Log.Information("User: {username}", Username);
            Log.Information("Password: {password}", Password);
            Log.Information("School: {school}", SchoolName);

            Type loginWorkerType = Utils.GetLoginWorkerByName(SchoolName);

            ILoginWorker loginWorker = null;
            if (loginWorkerType != null)
            {
                Log.Information("使用专门登录适配器 <{LoginWorkerTypeName}>.", loginWorkerType.Name);
                loginWorker = (ILoginWorker)Activator.CreateInstance(loginWorkerType);
            }
            else
            {
                Log.Information("使用通用登录适配器 <DefaultLoginWorker>.");
                loginWorker = new DefaultLoginWorker();
            }



            return await base.OnExecuteAsync(app);
        }
    }
}
