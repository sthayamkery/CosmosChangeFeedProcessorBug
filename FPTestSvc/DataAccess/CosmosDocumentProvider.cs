using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration.Internal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Linq;
using WK.FPTest.DataAccess.Interfaces;
using WK.FPTest.Models.Cosmos;
using WK.FPTest.Models.Tollbridge;
using WK.FPTest.Startup.Interfaces;

namespace WK.FPTest.DataAccess
{
    public class CosmosDocumentProvider : ICosmosDocumentProvider
    {
        private readonly ILogger _logger;
        private readonly CancellationToken _cancelToken;
        private IDocumentClient _documentClient;
        private CosmosDbSettingsCtx _cosmosSettings;
        private CosmosDbSettingsCtx _cosmosSettingsDest;
        private IConfiguration _config;

        public CosmosDocumentProvider(ILogger logger, CancellationToken cancelToken, IConfiguration config)
        {
            _cancelToken = cancelToken;
            _logger = logger;
            _config = config;
        }

        public async Task ReplaceDocumentAsync(Document doc)
        {
            if (_documentClient == null) InitializeClient();
            ReAttempt: // goto label lives matter ;P
            try
            {
                _logger.Verbose("Replacing batch form document link: {SelfLink}", doc.SelfLink);
                var response = await _documentClient.ReplaceDocumentAsync(doc);
            }
            //TimeSpan is a struct default = 00:00:00
            catch (DocumentClientException exception) when (exception.RetryAfter != default(TimeSpan))
            {
                _logger.Warning("{Operation} is exceeding rate limit, retry after {Milliseconds}ms", "Upsert", exception.RetryAfter.TotalMilliseconds);
                await Task.Delay(exception.RetryAfter, _cancelToken);
                goto ReAttempt;
            }
        }
        public async Task<bool> CopyDataToDestination(Document doc)
        {
            if (_documentClient == null) InitializeDestClient();
            ReAttempt:
            try
            {
                var uri = UriFactory.CreateDocumentCollectionUri(_cosmosSettingsDest.DatabaseName, _cosmosSettingsDest.CollectionName);
                await _documentClient.CreateDocumentAsync(uri, doc);
                return false;
            }
            catch (DocumentClientException exception) when (exception.RetryAfter != default(TimeSpan))
            {
                _logger.Warning("{Operation} is exceeding rate limit, retry after {Milliseconds}ms", "Upsert", exception.RetryAfter.TotalMilliseconds);
                await Task.Delay(exception.RetryAfter, _cancelToken);
                goto ReAttempt;
            }
        }

        public async Task<List<UserComment>> GetUserDataFromSource()
        {
            if (_documentClient == null)
            {
                InitializeClient();
                InitializeDestClient();
            }
            var collectionUri = UriFactory.CreateDocumentCollectionUri(_cosmosSettings.DatabaseName, _cosmosSettings.CollectionName);
            var option = new FeedOptions { EnableCrossPartitionQuery = true };
            var query = _documentClient.CreateDocumentQuery<UserComment>(collectionUri,
                    new SqlQuerySpec(string.Format("SELECT * FROM c")), option)
                .AsDocumentQuery();
            var results = new List<UserComment>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<UserComment>());
            }
            return results;
        }
        public async Task<List<UserComment>> GetUserDataFromDest()
        {
            if (_documentClient == null)
            {
                InitializeClient();
                InitializeDestClient();
            }
            var collectionUri = UriFactory.CreateDocumentCollectionUri(_cosmosSettingsDest.DatabaseName, _cosmosSettingsDest.CollectionName);
            var option = new FeedOptions { EnableCrossPartitionQuery = true };
            var query = _documentClient.CreateDocumentQuery<UserComment>(collectionUri,
                    new SqlQuerySpec(string.Format("SELECT * FROM c")), option)
                .AsDocumentQuery();
            var results = new List<UserComment>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<UserComment>());
            }
            return results;
        }
        public async Task<bool> AddUserData(List<UserComment> userDataList)
        {
            if (_documentClient == null) InitializeDestClient();
            var uri = UriFactory.CreateDocumentCollectionUri(_config.MonitoredDb, _config.MonitoredCollection);
            foreach (var userData in userDataList)
            {
                ReAttempt: 
                try
                {
                    await _documentClient.CreateDocumentAsync(uri, userData);
                }
                //TimeSpan is a struct default = 00:00:00
                catch (DocumentClientException exception) when (exception.RetryAfter != default(TimeSpan))
                {
                    _logger.Warning("{Operation} is exceeding rate limit, retry after {Milliseconds}ms", "Upsert", exception.RetryAfter.TotalMilliseconds);
                    await Task.Delay(exception.RetryAfter, _cancelToken);
                    goto ReAttempt;
                }
            }
            return true;
        }
        public async Task ClearData()
        {
            await DeleteDocumentsFromSource();
            await DeleteDocumentsFromDest();
        }
        private async Task<bool> DeleteDocumentsFromSource()
        {
            var results = await GetUserDataFromSource();
            if (_documentClient == null) InitializeClient();
            var i = 0;
            foreach (var userData in results)
            {
                i = i + 1;
                var opt = new RequestOptions { PartitionKey = new PartitionKey(userData.UserId) };
                var docUri = UriFactory.CreateDocumentUri(_config.MonitoredDb, _config.MonitoredCollection, userData.Id.ToString());
                await _documentClient.DeleteDocumentAsync(docUri, opt);
            }
            _logger.Information($"{i} record(s) deleted");
            return true;
        }
        private async Task<bool> DeleteDocumentsFromDest()
        {
            var results = await GetUserDataFromDest();
            if (_documentClient == null) InitializeDestClient();
            var i = 0;
            foreach (var userData in results)
            {
                i = i + 1;
                var opt = new RequestOptions { PartitionKey = new PartitionKey(userData.UserId) };
                var docUri = UriFactory.CreateDocumentUri(_config.DestDb, _config.DestCollection, userData.Id.ToString());
                await _documentClient.DeleteDocumentAsync(docUri, opt);
            }
            _logger.Information($"{i} record(s) deleted");
            return true;
        }
        private void InitializeClient()
        {
            if (_cosmosSettings == null )
            {
                _cosmosSettings = new CosmosDbSettingsCtx
                {
                    Uri = new Uri(_config.DocDbUri),
                    CollectionName = _config.MonitoredCollection,
                    DatabaseName = _config.MonitoredDb,
                    WriteMasterKey = _config.DocDbKey
                };
                _documentClient = new DocumentClient(_cosmosSettings.Uri, _cosmosSettings.WriteMasterKey);
            }
        }
        private void InitializeDestClient()
        {
            if (_cosmosSettingsDest == null)
            {
                _cosmosSettingsDest = new CosmosDbSettingsCtx
                {
                    Uri = new Uri(_config.DocDbUri),
                    CollectionName = _config.DestCollection,
                    DatabaseName = _config.DestDb,
                    WriteMasterKey = _config.DocDbKey
                };
                _documentClient = new DocumentClient(_cosmosSettingsDest.Uri, _cosmosSettingsDest.WriteMasterKey);
            }
        }
    }
}
