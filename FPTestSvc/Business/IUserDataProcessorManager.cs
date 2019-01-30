using Microsoft.Azure.Documents;
using Serilog;
using Serilog.Context;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using WK.FPTest.Business.Interfaces;
using WK.FPTest.DataAccess.Interfaces;
using WK.FPTest.Models.Cosmos;

namespace WK.FPTest.Business
{
    public class IUserDataProcessorManager : IUserDataProcessManager<Document>
    {
        private readonly ILogger _logger;
        private readonly IUserDataBatchManager _userDataBatchManager;
        private readonly ICosmosDocumentProvider _cosmosDocumentProvider;

        public IUserDataProcessorManager(IUserDataBatchManager userDataBatchManager,
            ICosmosDocumentProvider documentManager,
            ILogger logger)
        {
            _cosmosDocumentProvider = documentManager;
            _logger = logger;
            _userDataBatchManager = userDataBatchManager;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="host"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public async Task ProcessUserData(Document doc, string host, string instance)
        {
            if (doc == null) return;
            UserComment item = (dynamic)doc;
            try
            {
                using (LogContext.PushProperty("UserId", item.UserId))
                {
                    _logger.Information("Started processing records for {TrackingId} with {Status}", item.Id, item.Status);
                    var importResults = _userDataBatchManager.ProcessChangeFeedData(item);
                    doc.SetPropertyValue("consumer", instance);
                    doc.SetPropertyValue("host", host);
                    doc.SetPropertyValue("sourceid", item.Id);
                    doc.SetPropertyValue("id", Guid.NewGuid());//create a new id so that we can track duplicates
                    await _cosmosDocumentProvider.CopyDataToDestination(doc);
                    if (importResults != null)
                    {
                        doc.SetPropertyValue("status", importResults.Status);
                        await _cosmosDocumentProvider.ReplaceDocumentAsync(doc).ConfigureAwait(false);
                    }
                    await Task.Delay(TimeSpan.FromMilliseconds(2));
                    _logger.Information("Completed processing records for {TrackingId} with {Status}", item.Id, item.Status);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Batch Forms Import");
            }

        }
    }
}
