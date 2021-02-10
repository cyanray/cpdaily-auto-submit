using cpdaily_auto_submit.CpdailyModels;
using cpdaily_auto_submit.LoginWorkers;
using McMaster.Extensions.CommandLineUtils;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cpdaily_auto_submit
{
    [Command(Description = "提交表单")]
    class SubmitCommand : CommandBase
    {
        protected async override Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            if (AppConfig.Users.Count == 0)
            {
                Log.Error("没有找到任何用户，请执行 init 指令初始化配置文件。");
                return 1;
            }
            if (string.IsNullOrEmpty(AppConfig.SchoolName))
            {
                Log.Error("学校名称字段不可为空！请检查配置文件或执行 init 指令初始化配置文件！");
                return 1;
            }
            if (AppConfig.FormFields.Count == 0)
            {
                Log.Error("没有找到任何表单字段！请检查配置文件或执行 init 指令初始化配置文件！");
                return 1;
            }

            CpdailyCore cpdaily = new CpdailyCore();
            string SchoolName = AppConfig.SchoolName;
            SchoolDetails schoolDetails = null;
            ILoginWorker loginWorker = null;
            try
            {
                Log.Information("正在获取 {info} ...", "SecretKey");
                var secretKeyTask = cpdaily.GetSecretKeyAsync();
                Log.Information("正在获取 {info} ...", "学校列表");
                var schools = await cpdaily.GetSchoolsAsync();
                Log.Information("正在获取 {info} ...", "学校ID");
                var school = schools.Where(x => x.Name == SchoolName).FirstOrDefault();
                schoolDetails = await cpdaily.GetSchoolDetailsAsync(school, await secretKeyTask);

                Type loginWorkerType = Utils.GetLoginWorkerByName(SchoolName);
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
            }
            catch (Exception ex)
            {
                Log.Error("获取基本参数时出现异常!");
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                return 1;
            }

            for (int i = 0; i < AppConfig.Users.Count; i++)
            {
                string Username = AppConfig.Users[i].Username;
                string Password = AppConfig.Users[i].Password;
                string cookies = null;
                try
                {
                    Log.Information("正在登录 {username} ...", Username);
                    Log.Information("正在获取登录所需参数...");
                    var parameter = await loginWorker.GetLoginParameter(Username, Password, schoolDetails.GetIdsLoginUrl());
                    if (parameter.NeedCaptcha)
                    {
                        Log.Information("需要验证码!");
                        throw new Exception("需要验证码！暂时无法处理！");
                    }

                    Log.Information("正在登录...");
                    cookies = await loginWorker.IdsLogin(parameter);
                    Log.Information("登录成功, Cookie: {cookie}", cookies);

                }
                catch (Exception ex)
                {
                    Log.Error("登录过程中出现异常!");
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }

                try
                {
                    var formItems = await cpdaily.GetFormItemsAsync(schoolDetails.GetAmpUrl(), cookies);
                    Log.Information("找到了 {count} 个未填表单!", formItems.Length);
                    foreach (var form in formItems)
                    {
                        Log.Information("正在获取表单的字段...");
                        var formFields = await cpdaily.GetFormFieldsAsync(schoolDetails.GetAmpUrl(), cookies, form.WId, form.FormWId);
                        var requiredFields = formFields.Where(x => x.IsRequired == 1).ToArray();

                        if (requiredFields.Length != AppConfig.FormFields.Count)
                        {
                            var desc = $"配置文件中的表单字段数量({AppConfig.FormFields.Count})与需要的表单字段数量({requiredFields.Length})不一样!";
                            throw new Exception(desc);
                        }

                        for (int t = 0; t < requiredFields.Length; ++t)
                        {
                            requiredFields[t] = CpdailyCore.MergeToFormField(requiredFields[t], AppConfig.FormFields[t]);
                        }
                        Log.Information("提交表单中...");
                        await cpdaily.SubmitForm(schoolDetails.GetAmpUrl(), cookies, form, requiredFields, "Address", 0, 0);
                        Log.Information("表单提交成功!");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("提交表单时出现异常!");
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
            }

            return await base.OnExecuteAsync(app);
        }

    }
}
