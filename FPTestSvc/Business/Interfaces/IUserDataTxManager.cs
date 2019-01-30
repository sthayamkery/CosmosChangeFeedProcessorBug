using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WK.FPTest.Business.Interfaces
{
    public interface IUserDataTxManager
    {
        Task<bool> SeedData();
        Task<bool> NoDuplicatesWereCreated();

        Task<bool> ClearData();
    }
}
