using cpdaily_auto_submit.Models;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Serilog;
using System.IO;
using System.Threading.Tasks;

namespace cpdaily_auto_submit
{
    [HelpOption("-h|--help")]
    abstract class CommandBase
    {
        protected const string AppConfigPath = "AppConfig.json";

        private static AppConfig _appConfig = null;

        protected static AppConfig AppConfig
        {
            get
            {
                if (_appConfig is null) LoadAppConfig();
                return _appConfig;
            }
            set
            {
                _appConfig = value;
                SaveAppConfig();
            }
        }

        protected static void LoadAppConfig()
        {
            if (!File.Exists(AppConfigPath))
            {
                _appConfig = new AppConfig();
                return;
            }
            var text = File.ReadAllText(AppConfigPath);
            _appConfig = JsonConvert.DeserializeObject<AppConfig>(text);
        }

        protected static void SaveAppConfig()
        {
            File.WriteAllText(AppConfigPath, JsonConvert.SerializeObject(_appConfig, Formatting.Indented));
        }

        public CommandBase()
        {
            Log.Logger = new LoggerConfiguration()
                            .WriteTo.Console()
                            .CreateLogger();
        }

        protected virtual Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            return Task.FromResult(0);
        }
    }
}