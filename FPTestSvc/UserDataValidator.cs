using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Serilog;
using WK.FPTest.Business.Interfaces;
using WK.FPTest.DependencyInjection;
using WK.FPTest.Models.Cosmos;
using WK.FPTest.Startup.Interfaces;

namespace WK.FPTest
{
    public class UserDataValidator
    {
        private readonly StructureMap.IContainer _container;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger _logger;


        static UserDataValidator()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.EventLog("User data validator", "Application", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
#if DEBUG
                .WriteTo.Console()
#endif
                .CreateLogger();
        }
        public UserDataValidator()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _container = new StructureMap.Container(CommonRegistry.Instance);
            _container.Inject(_cancellationTokenSource);
            _logger = _container.GetInstance<ILogger>();
        }

        protected void OnStart(string[] args)
        {
            
            var txManager = _container.GetInstance<IUserDataTxManager>();
            //clear source and dest db
            Log.Information("Clearing existing data");
            if (!txManager.ClearData().Result)
            {
                Log.Information("Could not clear collections");
            }
            Log.Information("Transmitting data to source db");
            if (txManager.SeedData().Result)
            {
                Log.Information("Validating data copied from source db to destination db");

                if (!txManager.NoDuplicatesWereCreated().Result)
                {
                    Log.Information("Duplicate records created.");
                }
            }
        }


        protected void OnStop()
        {
            // Cancel the cancellation token
            _cancellationTokenSource.Cancel();
            Log.CloseAndFlush();
        }


    }
}
