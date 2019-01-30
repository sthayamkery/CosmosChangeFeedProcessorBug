using System.Collections.Generic;
using WK.FPTest.Models.Cosmos;

namespace WK.FPTest.Business.Interfaces
{
    public interface IUserDataBatchManager
    {
        UserComment ProcessChangeFeedData(UserComment data);
    }
}