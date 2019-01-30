using System.Threading.Tasks;

namespace WK.FPTest.Business.Interfaces
{
    public interface IFeedManager
    {
        Task StartObservingAsync(string hostName);

        Task StopObservingAsync();
    }
}
