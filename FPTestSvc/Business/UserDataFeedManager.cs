using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.Documents.ChangeFeedProcessor.PartitionManagement;
using Microsoft.Azure.Documents.Client;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using WK.FPTest.Business.Interfaces;
using WK.FPTest.Models.Tollbridge;
using WK.FPTest.Startup.Interfaces;

namespace WK.FPTest.Business
{
    public class UserDataFeedManager : IFeedManager
    {
        private const string _changeFeedDB = "fpLease";

        private readonly CancellationToken _cancelToken;
        private readonly ChangeFeedProcessorOptions _feedProcessorOptions;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing.IChangeFeedObserverFactory _docObserverFactory;
        private DocumentCollectionInfo _documentCollectionInfo;
        private DocumentCollectionInfo _destCollectionInfo;
        private DocumentCollectionInfo _leaseCollectionInfo;
        private IChangeFeedProcessor _changeFeedProcessor;

        public UserDataFeedManager(CancellationToken cancelToken, IConfiguration config, ILogger logger, Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing.IChangeFeedObserverFactory docObserverFactory)
        {
            _cancelToken = cancelToken;
            _config = config;
            _docObserverFactory = docObserverFactory;
            _logger = logger;
            _feedProcessorOptions = new ChangeFeedProcessorOptions
            {
                StartFromBeginning = true,
                LeaseAcquireInterval = TimeSpan.FromMinutes(5),
                LeaseExpirationInterval = TimeSpan.FromMinutes(5),
                FeedPollDelay = TimeSpan.FromSeconds(15),
                LeasePrefix = _config.HostName + "-",
                LeaseRenewInterval = TimeSpan.FromSeconds(270)
            };
        }

        private async Task Initialize(string hostName)
        {
            bool restart = false;
            CosmosDbSettingsCtx cosmosDbSettings = new CosmosDbSettingsCtx
            {
                Uri = new Uri(_config.DocDbUri),
                CollectionName = _config.MonitoredCollection,
                DatabaseName = _config.MonitoredDb,
                WriteMasterKey = _config.DocDbKey
            };
            if (_changeFeedProcessor != null)
            {
                await StopObservingAsync();
                restart = true;
            }

           
            _documentCollectionInfo = new DocumentCollectionInfo
            {
                Uri = cosmosDbSettings.Uri,
                MasterKey = cosmosDbSettings.WriteMasterKey,
                DatabaseName = cosmosDbSettings.DatabaseName,
                CollectionName = cosmosDbSettings.CollectionName
            };
            _destCollectionInfo = new DocumentCollectionInfo
            {
                Uri = cosmosDbSettings.Uri,
                MasterKey = cosmosDbSettings.WriteMasterKey,
                DatabaseName = cosmosDbSettings.DatabaseName,
                CollectionName = "rxcomments"
            };
            _leaseCollectionInfo = new DocumentCollectionInfo
            {
                Uri = cosmosDbSettings.Uri,
                MasterKey = cosmosDbSettings.WriteMasterKey,
                DatabaseName = _changeFeedDB,
                CollectionName = "leases"
            };
            await CreateCollectionIfNotExistsAsync(_destCollectionInfo.DatabaseName, _destCollectionInfo.CollectionName,
                _config.LeaseCosmosThroughput, "/userId");
            await CreateCollectionIfNotExistsAsync(_documentCollectionInfo.DatabaseName, _documentCollectionInfo.CollectionName,
                _config.LeaseCosmosThroughput, "/userId");
            await CreateLeaseCollectionIfNotExistsAsync(_changeFeedDB, _leaseCollectionInfo.CollectionName, _config.LeaseCosmosThroughput);

            if (restart) await StartObservingAsync(hostName);
        }

        public async Task StartObservingAsync(string hostName)
        {
            if (_changeFeedProcessor == null)
            {
                // Should only be a first time event
                await Initialize(hostName);
            }

            while (true)
            {
                try
                {
                    var builder = new ChangeFeedProcessorBuilder();
                    _changeFeedProcessor = await builder
                        .WithHostName(hostName) //This becomes the owner in the lease document. It has to be unique.
                        .WithFeedCollection(_documentCollectionInfo)
                        .WithLeaseCollection(_leaseCollectionInfo)
                        .WithProcessorOptions(_feedProcessorOptions)
                        .WithObserverFactory(_docObserverFactory)
                        .BuildAsync();

                    await _changeFeedProcessor.StartAsync();
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to read the batch process forms from database : {DatabaseName} and collection : {CollectionName}"
                        , _documentCollectionInfo.DatabaseName, _documentCollectionInfo.CollectionName);
                    _logger.Information("Sleeping for {Milliseconds} ms and will try again", 600000);
                    await Task.Delay(600000, _cancelToken);// Sleep for 10 minutes then try again
                }
            }
        }

        public async Task StopObservingAsync()
        {
            await _changeFeedProcessor?.StopAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="collectionName"></param>
        /// <param name="offerThroughput"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        private async Task CreateCollectionIfNotExistsAsync(string databaseName, string collectionName,
            int? offerThroughput, string partitionKey)
        {
            using (var client = new DocumentClient(_documentCollectionInfo.Uri, _documentCollectionInfo.MasterKey))
            {
                // connecting client
                await client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseName });

                // create collection if it does not exist
                PartitionKeyDefinition pkDefn = new PartitionKeyDefinition() { Paths = new Collection<string>() { partitionKey } };
                await client.CreateDocumentCollectionIfNotExistsAsync(
                    UriFactory.CreateDatabaseUri(databaseName),
                    new DocumentCollection { Id = collectionName,PartitionKey = pkDefn},
                    new RequestOptions { OfferThroughput = offerThroughput});
            }
        }

        private async Task CreateLeaseCollectionIfNotExistsAsync(string databaseName, string collectionName,
            int? offerThroughput)
        {
            using (var client = new DocumentClient(_documentCollectionInfo.Uri, _documentCollectionInfo.MasterKey))
            {
                // connecting client
                await client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseName });

                // create collection if it does not exist
                await client.CreateDocumentCollectionIfNotExistsAsync(
                    UriFactory.CreateDatabaseUri(databaseName),
                    new DocumentCollection { Id = collectionName },
                    new RequestOptions { OfferThroughput = offerThroughput });
            }
        }

    }
}
