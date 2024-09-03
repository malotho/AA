namespace SQLServiceWorker;

public partial class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var mainConnectionString = "YourMainConnectionStringHere";

        var dataSources = new List<(string SourceDbName, string ConnectionString)>
        {
            ("SourceDb1", "YourSourceConnectionString1"),
            ("SourceDb2", "YourSourceConnectionString2"),
            ("SourceDb3", "YourSourceConnectionString3")
        };
        IDataRetriever dataRetriever = new SqlDataRetriever();
        IDataInserter dataInserter = new SqlBulkInserter(mainConnectionString, batchSize: 10000);
        IProcessedRecordsTracker processedRecordsTracker = new SqliteProcessedRecordsTracker();

        var processor = new DataProcessor(dataRetriever, dataInserter, processedRecordsTracker);
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }




            await processor.ProcessDataAsync(dataSources);
            await Task.Delay(1000, stoppingToken);
        }
    }
}
