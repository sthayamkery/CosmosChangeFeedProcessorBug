using System.Threading;
using System.Threading.Tasks;
using Serilog;
using WK.FPTest.Business.Interfaces;
using WK.FPTest.DependencyInjection;
using WK.FPTest.Startup.Interfaces;

namespace WK.FPTest
{
    public class UserDataProcessor
    {
        private const string Source = "UserDataProcessor";
        private readonly StructureMap.IContainer _container;
        private readonly IConfiguration _config;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger _logger;

        
        static UserDataProcessor()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.EventLog(Source, "Application", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
#if DEBUG
                .WriteTo.Console()
#endif
                .CreateLogger();
        }

        
        public UserDataProcessor()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _container = new StructureMap.Container(CommonRegistry.Instance);
            _container.Inject(_cancellationTokenSource);
            _config = _container.GetInstance<IConfiguration>();
            _logger = _container.GetInstance<ILogger>();
        }

        
        protected void OnStart(string[] args)
        {
            Log.Information("{Source} has been started", Source);
            Task.Run(() =>
            {
                // Composing at this level gives a little more control
                var feedManager = _container.GetInstance<IFeedManager>();
                Log.Information("Instance {InstanceId} is running in {Host}", _config.InstanceId, _config.HostName);
                var hostName = "UserDataProcessor";
                feedManager.StartObservingAsync(hostName);
            });
        }

        
        protected void OnStop()
        {
            Log.Information("{Source} is being requested to stop", Source);
            // Cancel the cancellation token
            _cancellationTokenSource.Cancel();
            Log.CloseAndFlush();
        }
    }
}
