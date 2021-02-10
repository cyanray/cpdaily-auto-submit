using cpdaily_auto_submit.CpdailyModels;
using cpdaily_auto_submit.LoginWorkers;
using cpdaily_auto_submit.Models;
using McMaster.Extensions.CommandLineUtils;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cpdaily_auto_submit
{
    [Command(Description = "加入新用户")]
    class AddUserCommand : CommandBase
    {
        [Option("-u|--username", Description = "学号")]
        [Required]
        public string Username { get; set; }

        [Option("-p|--password", Description = "密码")]
        [Required]
        public string Password { get; set; }

        protected async override Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            if(string.IsNullOrEmpty(AppConfig.SchoolName) || AppConfig.Users.Count == 0)
            {
                Log.Error("学校名称或账号列表为空! 请先执行 init 指令初始化配置文件。");
                return 1;
            }

            CpdailyCore cpdaily = new CpdailyCore();
            string SchoolName = AppConfig.SchoolName;
            string cookies = null;
            SchoolDetails schoolDetails = null;
            try
            {
                Log.Information("正在获取 {info} ...", "SecretKey");
                var secretKeyTask = cpdaily.GetSecretKeyAsync();
                Log.Information("正在获取 {info} ...", "学校列表");
                var schools = await cpdaily.GetSchoolsAsync();
                Log.Information("正在获取 {info} ...", "学校ID");
                var school = schools.Where(x => x.Name == SchoolName).FirstOrDefault();
                var schoolDetailsTask = cpdaily.GetSchoolDetailsAsync(school, await secretKeyTask);

                Type loginWorkerType = Utils.GetLoginWorkerByName(SchoolName);
                ILoginWorker loginWorker = null;
                if (loginWorkerType != null)
                {
                    Log.Information("使用专门登录适配器 <{LoginWorkerTypeName}>", loginWorkerType.Name);
                    loginWorker = (ILoginWorker)Activator.CreateInstance(loginWorkerType);
                }
                else
                {
                    Log.Information("使用通用登录适配器 <{LoginWorkerTypeName}>", "DefaultLoginWorker");
                    loginWorker = new DefaultLoginWorker();
                }

                Log.Information("正在获取登录所需参数...");
                schoolDetails = await schoolDetailsTask;
                var parameter = await loginWorker.GetLoginParameter(Username, Password, schoolDetails.GetIdsLoginUrl());
                if (parameter.NeedCaptcha)
                {
                    Log.Information("需要验证码!");
                    throw new Exception("需要验证码！暂时无法处理！");
                }

                Log.Information("正在登录...");
                cookies = await loginWorker.IdsLogin(parameter);
                Log.Information("登录成功, Cookie: {cookie}", cookies);

                // remove before adding to avoid duplication.
                AppConfig.Users.RemoveAll(x => x.Username == Username);
                AppConfig.Users.Add(new User() { Username = Username, Password = Password });
                SaveAppConfig();
            }
            catch (Exception ex)
            {
                Log.Error("登录过程中出现异常!");
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                return 1;
            }
            return await base.OnExecuteAsync(app);
        }
    }
}
