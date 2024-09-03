using Microsoft.Data.Sqlite;

namespace SQLServiceWorker;

public class SqliteProcessedRecordsTracker : IProcessedRecordsTracker
{
    private readonly string _connectionString = "Data Source=processed_records.db";

    public SqliteProcessedRecordsTracker()
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            string tableCreationQuery = @"
                CREATE TABLE IF NOT EXISTS ProcessedRecords (
                    SourceDbName TEXT PRIMARY KEY,
                    LastProcessedId INTEGER,
                    LastProcessedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );";

            using (var command = new SqliteCommand(tableCreationQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }

    public int GetLastProcessedId(string sourceDbName)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT LastProcessedId FROM ProcessedRecords WHERE SourceDbName = @SourceDbName";
                command.Parameters.AddWithValue("@SourceDbName", sourceDbName);

                var result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }
    }

    public void UpdateLastProcessedId(string sourceDbName, int lastProcessedId)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO ProcessedRecords (SourceDbName, LastProcessedId)
                    VALUES (@SourceDbName, @LastProcessedId)
                    ON CONFLICT(SourceDbName) 
                    DO UPDATE SET LastProcessedId = @LastProcessedId, LastProcessedAt = CURRENT_TIMESTAMP";

                command.Parameters.AddWithValue("@SourceDbName", sourceDbName);
                command.Parameters.AddWithValue("@LastProcessedId", lastProcessedId);

                command.ExecuteNonQuery();
            }
        }
    }
}

