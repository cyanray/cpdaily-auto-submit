using cpdaily_auto_submit.CpdailyModels;
using cpdaily_auto_submit.LoginWorkers;
using cpdaily_auto_submit.Models;
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
            Log.Information("School: {school}", SchoolName);

            CpdailyCore cpdaily = new CpdailyCore();

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
                var schoolDetails = await schoolDetailsTask;
                var parameter = await loginWorker.GetLoginParameter(Username, Password, schoolDetails.GetIdsLoginUrl());
                if (parameter.NeedCaptcha)
                {
                    Log.Information("需要验证码!");
                    throw new Exception("需要验证码！暂时无法处理！");
                }

                Log.Information("正在登录...");
                var cookies = await loginWorker.IdsLogin(parameter);
                Log.Information("登录成功, Cookie: {cookie}", cookies);

                // remove before adding to avoid duplication.
                AppConfig.Users.RemoveAll(x => x.Username == Username);
                AppConfig.Users.Add(new User() { Username = Username, Password = Password });
                AppConfig.SchoolName = SchoolName;
                SaveAppConfig();
            }
            catch (Exception ex)
            {
                Log.Error("登录过程中出现异常!");
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
            }


            return await base.OnExecuteAsync(app);
        }
    }
}
