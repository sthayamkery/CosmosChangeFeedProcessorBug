using System;
using System.Reflection;
using WK.FPTest;

namespace FPTestConsole
{
    class Program
    {
        static void Main()
        {
            Console.Title = "User Data processor"; 
            var fpTestSvc = new UserDataProcessor();

            var m = fpTestSvc.GetType().GetMethod("OnStart", BindingFlags.NonPublic | BindingFlags.Instance);
            if (m == null) return;
            m.Invoke(fpTestSvc, new object[] { new string[] { } });

            System.Console.ReadKey(true);

            m = fpTestSvc.GetType().GetMethod("OnStop", BindingFlags.NonPublic | BindingFlags.Instance);
            if (m == null) return;
            m.Invoke(fpTestSvc, new object[] { });
        }
    }
}
