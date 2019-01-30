namespace WK.FPTest.Startup.Interfaces
{
    public interface IConfiguration
    {
        string HostName { get; }
        string InstanceId { get; }
        int? LeaseCosmosThroughput { get; }
        string DocDbKey { get; }
        string DocDbUri { get; }
        string MonitoredDb { get; }
        string MonitoredCollection { get; }
        string DestDb { get; }
        string DestCollection { get; }

        string LeaseDb { get; }
        string LeaseCollection { get; }
    }
}
