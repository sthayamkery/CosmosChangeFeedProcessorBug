using System;
using System.Configuration;
using WK.FPTest.Startup.Interfaces;

namespace WK.FPTest.Startup
{
    public sealed class Configuration : IConfiguration
    {
        public int? LeaseCosmosThroughput =>
            Convert.ToInt32(ConfigurationManager.AppSettings["CosmosThroughput"] ?? "500");

        public string DocDbKey => ConfigurationManager.AppSettings["DocDbKey"];
        public string DocDbUri => ConfigurationManager.AppSettings["DocDbUri"];
        public string MonitoredDb => ConfigurationManager.AppSettings["MonitoredDb"];
        public string MonitoredCollection => ConfigurationManager.AppSettings["MonitoredCollection"];
        public string DestDb => ConfigurationManager.AppSettings["DestDb"];
        public string DestCollection => ConfigurationManager.AppSettings["DestCollection"];
        public string LeaseDb => ConfigurationManager.AppSettings["LeaseDb"];
        public string LeaseCollection => ConfigurationManager.AppSettings["LeaseCollection"];

        public string HostName => ConfigurationManager.AppSettings["HostName"];
        public string InstanceId => Guid.NewGuid().ToString();
    }
        //public string InstanceId => ConfigurationManager.AppSettings["InstanceId"]
    
}
