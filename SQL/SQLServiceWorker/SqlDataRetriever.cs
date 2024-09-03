using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

public class SqlDataRetriever : IDataRetriever
{
    public async Task<IEnumerable<Received>> RetrieveDataAsync(int lastProcessedId, int batchSize, string connectionString)
    {
        var results = new List<Received>();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand("usp_GetReceivedData", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@LastProcessedId", lastProcessedId);
                command.Parameters.AddWithValue("@BatchSize", batchSize);

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        results.Add(new Received
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            ReFromNum = reader.GetString(reader.GetOrdinal("re_fromnum")),
                            ReMessage = reader.GetString(reader.GetOrdinal("re_message"))
                        });
                    }
                }
            }
        }

        return results;
    }
}
