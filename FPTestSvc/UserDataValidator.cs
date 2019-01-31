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

        protected async Task OnStart(string[] args)
        {
            var txManager = _container.GetInstance<IUserDataTxManager>();
            for (int i = 0; i < 20; i++)
            {
                //clear source and dest db
                Log.Information("Clearing existing data");
                if (!await txManager.ClearData())
                {
                    Log.Information("Could not clear collections");
                }
                Log.Information("Transmitting data to source db");
                if (await txManager.SeedData())
                {
                    Log.Information("Validating data copied from source db to destination db");

                    if (!await txManager.NoDuplicatesWereCreated())
                    {
                        Log.Information("Duplicate records created.");
                    }
                }
                Log.Information("Waiting for next run");
                Task.Delay(TimeSpan.FromSeconds(310));
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
