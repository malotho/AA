using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DataProcessor
{
    private readonly IDataRetriever _dataRetriever;
    private readonly IDataInserter _dataInserter;
    private readonly IProcessedRecordsTracker _processedRecordsTracker;
    private readonly int _batchSize;

    public DataProcessor(IDataRetriever dataRetriever, IDataInserter dataInserter, IProcessedRecordsTracker processedRecordsTracker, int batchSize = 10000)
    {
        _dataRetriever = dataRetriever;
        _dataInserter = dataInserter;
        _processedRecordsTracker = processedRecordsTracker;
        _batchSize = batchSize;
    }

    public async Task ProcessDataAsync(IEnumerable<(string SourceDbName, string ConnectionString)> dataSources)
    {
        foreach (var dataSource in dataSources)
        {
            string sourceDbName = dataSource.SourceDbName;
            string connectionString = dataSource.ConnectionString;

            int lastProcessedId = _processedRecordsTracker.GetLastProcessedId(sourceDbName);
            bool moreData = true;

            while (moreData)
            {
                var dataBatch = await _dataRetriever.RetrieveDataAsync(lastProcessedId, _batchSize, connectionString);

                if (dataBatch == null || !dataBatch.Any())
                {
                    moreData = false;
                }
                else
                {
                    _dataInserter.InsertData(dataBatch);
                    lastProcessedId = dataBatch.Max(r => r.Id);
                    _processedRecordsTracker.UpdateLastProcessedId(sourceDbName, lastProcessedId);
                }
            }
        }
    }
}
