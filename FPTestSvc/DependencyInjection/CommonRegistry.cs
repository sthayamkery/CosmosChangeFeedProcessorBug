using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing;
using StructureMap;
using WK.FPTest.Business;
using WK.FPTest.DataAccess;
using WK.FPTest.Startup;
using Serilog;
using System.Threading;
using WK.FPTest.Business.Interfaces;
using WK.FPTest.DataAccess.Interfaces;
using WK.FPTest.Observer;
using WK.FPTest.Startup.Interfaces;

namespace WK.FPTest.DependencyInjection
{
    public class CommonRegistry : Registry
    {
        public static Registry Instance => new CommonRegistry();
        public CommonRegistry()
        {
            For<ILogger>().Use(() => Log.Logger);
            For<CancellationToken>().Use((container) => container.GetInstance<CancellationTokenSource>().Token).Singleton();

            For<IConfiguration>().Use<Configuration>().Singleton();
            For<IChangeFeedObserver>().Use<FPTestObserver>();
            For<IChangeFeedObserverFactory>().Use<FpTestFeedObserverFactory>();
            For<IFeedManager>().Use<UserDataFeedManager>();
            For<IUserDataBatchManager>().Use<UserDataBatchManager>();
            For<ICosmosDocumentProvider>().Use<CosmosDocumentProvider>().Singleton();
            For<IUserDataProcessManager<Document>>().Use<IUserDataProcessorManager>();
            For<IUserDataTxManager>().Use<UserDataTxManager>();
        }
    }
}
