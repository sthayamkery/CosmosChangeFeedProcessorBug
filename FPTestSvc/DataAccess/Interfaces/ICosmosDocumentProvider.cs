using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using WK.FPTest.Models.Cosmos;

namespace WK.FPTest.DataAccess.Interfaces
{
    public interface ICosmosDocumentProvider
    {
        Task ReplaceDocumentAsync(Document doc);
        Task<bool> CopyDataToDestination(Document doc);
        Task<List<UserComment>> GetUserDataFromSource();
        Task<List<UserComment>> GetUserDataFromDest();
        Task<bool> AddUserData(List<UserComment> userDataList);
        Task ClearData();
    }
}
