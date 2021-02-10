using cpdaily_auto_submit.CpdailyModels;
using cpdaily_auto_submit.LoginWorkers;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace cpdaily_auto_submit
{
    [Command(
    Name = "cpdaily-auto-submit",
    Description = "今日校园自动填报程序",
    ExtendedHelpText = @"
提示:
  本程序采用 MIT 协议开源(https://github.com/cyanray/cpdaily-auto-submit).
  任何人可免费使用本程序并查看其源代码.
")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(
        typeof(InitCommand),
        typeof(AddUserCommand)
    )]
    class Program : CommandBase
    {
        static async Task<int> Main(string[] args)
        {
            return await CommandLineApplication.ExecuteAsync<Program>(args);
        }

        private static string GetVersion()
    => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        protected async override Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            app.ShowHelp();
            return await base.OnExecuteAsync(app);
        }
    }
}
