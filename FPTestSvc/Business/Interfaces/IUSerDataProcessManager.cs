using System.Threading.Tasks;

namespace WK.FPTest.Business.Interfaces
{
    public interface IUserDataProcessManager<in T>
    {
        Task ProcessUserData(T item, string host, string instance);
    }
}
