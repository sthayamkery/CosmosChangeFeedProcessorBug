using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WK.FPTest;

namespace UserDataTxConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title="User Data validator";
            var userDataValidator = new UserDataValidator();
            var m = userDataValidator.GetType().GetMethod("OnStart", BindingFlags.NonPublic | BindingFlags.Instance);
            if (m == null) return;
            m.Invoke(userDataValidator, new object[] { new string[] { } });

            System.Console.ReadKey(true);

            m = userDataValidator.GetType().GetMethod("OnStop", BindingFlags.NonPublic | BindingFlags.Instance);
            if (m == null) return;
            m.Invoke(userDataValidator, new object[] { });
        }

        
    }
}
