using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using WK.FPTest.Business.Interfaces;
using WK.FPTest.DataAccess.Interfaces;
using WK.FPTest.Models.Cosmos;

namespace WK.FPTest.Business
{
    public class UserDataTxManager:IUserDataTxManager
    {
        private readonly ILogger _logger;
        private readonly ICosmosDocumentProvider _cosmosDocumentProvider;
        public UserDataTxManager(ICosmosDocumentProvider cosmosDocumentProvider, ILogger logger)
        {
            _cosmosDocumentProvider = cosmosDocumentProvider;
            _logger = logger;
        }
        public async Task<bool> NoDuplicatesWereCreated()
        {
            var result = await GetUserDataFromDestination();
            _logger.Information("{Count} records fetched from destination database",result.Count);
            if (result.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                _logger.Information("Please run the Data processor before running the validator");
                return true;
            }
            if (result.Count > 62)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                _logger.Information("Duplicate data created. Expected count was 62 records. Actual count is {Count}",result.Count);
                return false;
            }
            return result.Count <= 62;
        }

        public async Task<bool> ClearData()
        {
            await _cosmosDocumentProvider.ClearData();
            return true;
        }

        public async Task<bool> SeedData()
        {
            _logger.Information("Adding data to source db");
            var userIds = new[] { 1, 11, 2, 4, 5, 8 };
            var lst = new List<UserComment>();
            foreach (var seed in userIds)
            {
                var userId = 5000 + seed;
                for (int i = 0; i < seed; i++)
                {
                    var data = new UserComment
                    {
                        UserId = userId,
                        FirmId = 8200 + i,
                        Comment = "Random quote " + i,
                        Status = "New",
                        Id = Guid.NewGuid()
                    };
                    lst.Add(data);
                }
            }

            await _cosmosDocumentProvider.AddUserData(lst);
            _logger.Information("31 record(s) added to source db");
            return true;
        }
        private async Task<List<UserComment>> GetUserDataFromDestination()
        {
            return await _cosmosDocumentProvider.GetUserDataFromDest();
        }
    }
}
