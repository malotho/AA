public interface IDataRetriever
{
    Task<IEnumerable<Received>> RetrieveDataAsync(int lastProcessedId, int batchSize, string connectionString);
}


public interface IDataInserter
{
    void InsertData(IEnumerable<Received> data);
}

public interface IProcessedRecordsTracker
{
    int GetLastProcessedId(string sourceDbName);
    void UpdateLastProcessedId(string sourceDbName, int lastProcessedId);
}