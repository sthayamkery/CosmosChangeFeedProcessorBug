using System;
using System.Configuration;
using Serilog;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using WK.Axcess.Workstream.BatchFormsImport.Business.Interfaces;
using WK.Axcess.Workstream.BatchFormsImport.DependencyInjection;
using WK.Axcess.Workstream.BatchFormsImport.Startup.Interfaces;

namespace WK.Axcess.Workstream.BatchFormsImport
{
    public class BatchFormsImportService : ServiceBase
    {
        private const string Source = "FPTest";
        private readonly StructureMap.IContainer _container;
        private readonly IConfiguration _config;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger _logger;

        /// <inheritdoc />
        /// <summary>
        /// This a static method for batch forms import service.
        /// </summary>
        static BatchFormsImportService()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.EventLog(Source, "Application", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
#if DEBUG
                .WriteTo.Console()
#endif
                .CreateLogger();
        }

        /// <inheritdoc />
        /// <summary>
        /// This a constructor for batch forms import service.
        /// </summary>
        public BatchFormsImportService()
        {
            ServiceName = "FPTest";
            _cancellationTokenSource = new CancellationTokenSource();
            _container = new StructureMap.Container(CommonRegistry.Instance);
            _container.Inject(_cancellationTokenSource);
            _config = _container.GetInstance<IConfiguration>();
            _logger = _container.GetInstance<ILogger>();
        }

        /// <inheritdoc />
        /// <summary>
        /// This method is called on start of the batch forms import service.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            Log.Information("{Source} has been started", Source);
            Task.Run(() =>
            {
                _container.GetInstance<ILogAppInsights>().Init(Source);
                // Composing at this level gives a little more control
                var feedManager = _container.GetInstance<IFeedManager>();
                Log.Information("Instance {InstanceId} is running in {Host}", _config.InstanceId, _config.HostName);
                var hostName = "FPTest.ChangeFeedProcessor";
                feedManager.StartObservingAsync(hostName);
            });
        }

        /// <inheritdoc />
        /// <summary>
        /// This method is called on stop of the batch forms import service.
        /// </summary>
        protected override void OnStop()
        {
            Log.Information("{Source} is being requested to stop", Source);
            // Cancel the cancellation token
            _cancellationTokenSource.Cancel();
            Log.CloseAndFlush();
        }

        /// <inheritdoc />
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Container?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
