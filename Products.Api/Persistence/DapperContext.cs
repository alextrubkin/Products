using System.Data;
using Microsoft.Data.SqlClient;

namespace Products.Api.Persistence;

internal class DapperContext(string connectionString)
{
    public IDbConnection CreateConnection()
    {
        return new SqlConnection(connectionString);
    }
}