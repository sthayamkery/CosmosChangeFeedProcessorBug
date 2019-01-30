using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing;
using Serilog;
using Serilog.Context;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WK.FPTest.Business.Interfaces;
using WK.FPTest.Startup.Interfaces;

namespace WK.FPTest.Observer
{
    public class FPTestObserver : IChangeFeedObserver
    {
        private readonly IUserDataProcessManager<Document> _batchImportManager;
        private readonly CancellationToken _cancelToken;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        public FPTestObserver(IUserDataProcessManager<Document> importManager, ILogger logger,
            CancellationToken cancelToken, IConfiguration config)
        {
            _batchImportManager = importManager;
            _cancelToken = cancelToken;
            _config = config;
            _logger = logger;
        }

        public Task OpenAsync(IChangeFeedObserverContext context)
        {
            _logger.Verbose("OpenAsych called from observer: Partition Key Range = {PartitionKeyRangeId}", context.PartitionKeyRangeId);
            return Task.CompletedTask;
        }

        public Task CloseAsync(IChangeFeedObserverContext context, ChangeFeedObserverCloseReason reason)
        {
            _logger.Verbose("CloseAsych called from observer: Partition Key Range = {PartitionKeyRangeId}, Reason = {Reason}", context.PartitionKeyRangeId, reason);
            return Task.CompletedTask;
        }

        public async Task ProcessChangesAsync(IChangeFeedObserverContext context, IReadOnlyList<Document> docs, CancellationToken _token)
        {
            using (LogContext.PushProperty("PartitionKeyRangeId", context.PartitionKeyRangeId))
            {
                _logger.Verbose("ProcessChangeAsync called from observer: {Documents} document(s) to process", docs.Count);
                await ProcessChangesHandler(docs, _token);
            }
            await Task.CompletedTask;
        }

        private async Task ProcessChangesHandler(IReadOnlyList<Document> formDocuments, CancellationToken token)
        {
            var position = "";
            token.Register(() => CancellationTokenFired(formDocuments, position));
            var projectForms = formDocuments.ToList();
            var total = projectForms.Count();
            var index = 0;
            _logger.Verbose("Filtered down to {Documents} document(s) that are not in a {Status} status", total, "Final State");

            projectForms.ForEach(async userData =>
            {
                _cancelToken.ThrowIfCancellationRequested();
                token.ThrowIfCancellationRequested();
                _logger.Verbose("Importing {Index} of out {Total} Forms", (++index), total);
                _logger.Information("Data received {data}",JsonConvert.SerializeObject(userData));
                await _batchImportManager.ProcessUserData(userData, _config.HostName, _config.InstanceId);
            });
            await Task.CompletedTask;
        }
        private void CancellationTokenFired(IReadOnlyList<Document> formDocuments, string position)
        {
            _logger.Information("Cancellation token invoked in ProcessChangesHandler when processing {Form}", position);
            var feed = formDocuments
                .Select(y => new { trackingid = y.GetPropertyValue<string>("userid"), status = y.GetPropertyValue<string>("status"), formid = y.GetPropertyValue<string>("formid") }).ToList();
            _logger.Verbose("Change feed cancelled: {@Feed}", feed);
        }
    }
}
