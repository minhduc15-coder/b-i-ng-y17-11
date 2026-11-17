using Microsoft.Data.SqlClient;
namespace RetailSalesSystem.DAL;
public class Db
{
    private readonly string _connectionString;
    public Db(string connectionString) => _connectionString = connectionString;
    public SqlConnection CreateConnection() => new(_connectionString);
}
