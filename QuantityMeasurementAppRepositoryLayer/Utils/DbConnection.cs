using Microsoft.Data.SqlClient;

namespace QuantityMeasurementAppRepositoryLayer.Utils
{
    public static class DbConnectionFactory
    {
        private static readonly string connectionString =
            "Server=DESKTOP-9IKMOAJS;Database=QuantityMeasurementDB;Trusted_Connection=True;TrustServerCertificate=True";

        public static SqlConnection CreateConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
