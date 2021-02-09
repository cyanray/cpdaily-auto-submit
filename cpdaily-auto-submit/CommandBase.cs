using McMaster.Extensions.CommandLineUtils;
using Serilog;
using System.Threading.Tasks;

namespace cpdaily_auto_submit
{
    [HelpOption("-h|--help")]
    abstract class CommandBase
    {
        protected const string AppConfigPath = "AppConfig.json";

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