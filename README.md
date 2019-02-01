# Introduction 
This is a sample project which uses the  DocumentDB ChangeFeedProcessor SDK (Microsoft.Azure.DocumentDB.ChangeFeedProcessor v 2.2.5). This project can replicate a defect in the SDK, where the change feed is being pushed to all processes instead of distributing it.  

UserDataProcessor listens to the change feed, processes the data and updates cosmosdb. Multiple instances of the processor will be running. The change feed should be pushed to each instance in a distributed fashion. Sometimes we can see the feed being pushed to all instances. This usually occurs after a "leaselostexception" is recorded. The logs will show messages similar to the following.

[15:01:40 INF] Partition 0 lease update conflict. Reading the current version of lease.
[15:01:40 INF] Partition 0 update failed because the lease with token '"00000000-0000-0000-b8de-78bc609701d4"' was updated by host 'UserDataProcessor' with token '"00000000-0000-0000-b8de-fd1b992201d4"'. Will retry, 5 retry(s) left.


UserDataTx is a sample application which inserts data(31 records) to the monitored collection (comments). The date insertion is done every 5 minutes. Before inserting the data, existing data is cleared. The data insert will trigger the change feed. Userprocessor records the received change feed into a new collection (rxComments). It then processes the received data and updates the monitored collection. If everything works well the new collection should have 62 records (31 records with status "New" and 31 records with status "Processed"). In most cases we are seeing more than 62 records. 

# Getting Started
Open the UserDataProcessor.sln and restore Nuget packages

This sample uses the local Cosmos emulator

*Database Names: fpData, fpLease (You can find in App.Config)
*Collection Names: comments(Monitored Collection), rxComments(Records all the change feeds received), leases 

# Build and Test
Set the project UserDataProcessorConsole as start up project. Run multiple instances(2 or more). Once the UserDataProcessor consoles are running, set the startup project as UserDataTxConsole. Start one instance of UserDataTxConsole. If duplicate records are created in the collection rxComments the logs will show the following message.
[14:52:21 INF] Duplicate data created. Expected count was 62 records. Actual count is 93
[14:52:21 INF] Duplicate records created.



