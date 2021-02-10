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
                AppConfig.SchoolName = SchoolName;
                SaveAppConfig();
            }
            catch (Exception ex)
            {
                Log.Error("登录过程中出现异常!");
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                return 1;
            }

            try
            {
                Log.Warning("下面进行表单向导，模拟完成一次历史表单，让程序学习如何填写表单。");
                Log.Information("获取历史表单...");
                var forms = await cpdaily.GetFormItemsHistoryAsync(schoolDetails.GetAmpUrl(), cookies);
                if (forms.Length == 0)
                {
                    throw new Exception("没有获取到历史表单，表单向导无法继续!");
                }
                Log.Information("获取到 {count} 条历史表单记录，请选择其中一条(输入序号):", forms.Length);
                for (int i = 0; i < forms.Length; i++)
                {
                    Log.Information("{No}. {Title}", i + 1, forms[i].Title);
                }
                int index = Convert.ToInt32(Console.ReadLine()) - 1;
                FormItem form = forms[index];
                var formFields = await cpdaily.GetFormFieldsAsync(schoolDetails.GetAmpUrl(),cookies, form.WId, form.FormWId);
                var requiredFields = formFields.Where(x => x.IsRequired == 1).ToArray();
                Log.Information("获取到 {count} 条必填字段", requiredFields.Length);
                List<FormFieldChange> result = new List<FormFieldChange>();
                for (int i = 0; i < requiredFields.Length; i++)
                {
                    FormField field = requiredFields[i];
                    var typeString = field.FieldType switch
                    {
                        1 => "填空",
                        2 => "单选",
                        3 => "多选",
                        4 => "图片",
                        _ => "未知",
                    };
                    Log.Information("{No}. {Title} ({type}):", i + 1, field.Title, typeString);
                    Log.Information("描述: {Description}", string.IsNullOrEmpty(field.Description) ? "无" : field.Description);
                    if (field.FieldType == 1)
                    {
                        Log.Information("请输入文本:");
                        string value = Console.ReadLine();
                        var c = new FormFieldChange()
                        {
                            FieldType = field.FieldType,
                            Title = field.Title,
                            Value = value
                        };
                        result.Add(c);
                    }
                    else if(field.FieldType == 2)
                    {
                        for (int t = 0; t < field.FieldItems.Count; t++)
                        {
                            FieldItem item = field.FieldItems[t];
                            Log.Information("\t{No}.{Title}", t + 1, item.Content);
                        }
                        Log.Information("请输入选项序号:");
                        int value = Convert.ToInt32(Console.ReadLine()) - 1;
                        var c = new FormFieldChange()
                        {
                            FieldType = field.FieldType,
                            Title = field.Title,
                            Value = field.FieldItems[value].Content
                        };
                        result.Add(c);
                    }
                    else
                    {
                        throw new Exception("暂不支持这种类型，请到 Github 提出 issues!");
                    }
                }
                Log.Information("表单向导完成！");

                AppConfig.FormFields = result;
                SaveAppConfig();
            }
            catch (Exception ex)
            {
                Log.Error("表单向导过程中出现异常!");
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                return 1;
            }

            return await base.OnExecuteAsync(app);
        }
    }
}
