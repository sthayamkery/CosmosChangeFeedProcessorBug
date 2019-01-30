using Serilog;
using WK.FPTest.Business.Interfaces;
using WK.FPTest.Models.Cosmos;

namespace WK.FPTest.Business
{
    public class UserDataBatchManager : IUserDataBatchManager
    {
        private readonly ILogger _logger;

        public UserDataBatchManager(ILogger logger)
        {
            _logger = logger;
        }

        public UserComment ProcessChangeFeedData(UserComment data)
        {
            _logger.Information("Processing {TrackingId} with status {Status} for User: {UserId}", data.Id,data.Status, data.UserId);
            // Retrieve the FormID
            if (data.Status == "New")
            {
                data.Status = "Processed";
                return data;
            }
            return null;
        }

    }
}
