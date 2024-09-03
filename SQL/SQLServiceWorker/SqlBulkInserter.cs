namespace SQLServiceWorker;

using System.Data;
using Microsoft.Data.SqlClient;

public partial class Worker
{
    public class SqlBulkInserter : IDataInserter
    {
        private readonly string _connectionString;
        private readonly int _batchSize;

        public SqlBulkInserter(string connectionString, int batchSize = 100000)
        {
            _connectionString = connectionString;
            _batchSize = batchSize;
        }

        public void InsertData(IEnumerable<Received> data)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                        {
                            bulkCopy.DestinationTableName = "received_total";
                            bulkCopy.BatchSize = _batchSize;

                            bulkCopy.ColumnMappings.Add("ReFromNum", "rt_msisdn");
                            bulkCopy.ColumnMappings.Add("ReMessage", "rt_message");

                            DataTable dataTable = new DataTable();
                            dataTable.Columns.Add("rt_msisdn", typeof(string));
                            dataTable.Columns.Add("rt_message", typeof(string));

                            foreach (var rec in data)
                            {
                                dataTable.Rows.Add(rec.ReFromNum, rec.ReMessage);
                            }

                            bulkCopy.WriteToServer(dataTable);
                        }

                        transaction.Commit();  // Commit transaction if successful
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();  // Rollback transaction on error
                        throw;  // Re-throw the exception after rollback
                    }
                }
            }
        }

        public void UpdateLastProcessedId(string sourceDbName, int lastProcessedId)
        {
            using (var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=processed_records.db"))
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
}
