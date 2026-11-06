using Microsoft.Data.SqlClient;
using ProductManagement.Web.Models;
using System.Data;

namespace ProductManagement.Web.Repositories
{
    public class SqlProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        public SqlProductRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string is unconfigured.");
        }

        public async Task<IReadOnlyList<Product>> GetAllAsync(string? searchTerm = null, string? category = null, string? status = null)
        {
            var list = new List<Product>();

            var sqlBuilder = new System.Text.StringBuilder("""
                SELECT [Id], [Sku], [Title], [Category], [Description],
                       [ImageUrl], [Price], [StockQuantity], [Status], [CreatedAt]
                FROM [dbo].[Products]
                WHERE 1 = 1
                """);

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand();
            command.Connection = connection;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sqlBuilder.Append(" AND ([Title] LIKE @Search OR [Sku] LIKE @Search OR [Description] LIKE @Search)");
                command.Parameters.Add("@Search", SqlDbType.NVarChar, 100).Value = $"%{searchTerm.Trim()}%";
            }

            if (!string.IsNullOrWhiteSpace(category) && category != "All")
            {
                sqlBuilder.Append(" AND [Category] = @Category");
                command.Parameters.Add("@Category", SqlDbType.NVarChar, 60).Value = category.Trim();
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                sqlBuilder.Append(" AND [Status] = @Status");
                command.Parameters.Add("@Status", SqlDbType.NVarChar, 40).Value = status.Trim();
            }

            sqlBuilder.Append(" ORDER BY [Id] DESC;");
            command.CommandText = sqlBuilder.ToString();

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(MapProduct(reader));
            }

            return list;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            const string sql = """
                SELECT [Id], [Sku], [Title], [Category], [Description],
                       [ImageUrl], [Price], [StockQuantity], [Status], [CreatedAt]
                FROM [dbo].[Products]
                WHERE [Id] = @Id;
                """;

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapProduct(reader);
            }

            return null;
        }

        public async Task<int> CreateAsync(Product product)
        {
            const string sql = """
                INSERT INTO [dbo].[Products]
                    ([Sku], [Title], [Category], [Description],
                     [ImageUrl], [Price], [StockQuantity], [Status], [CreatedAt])
                OUTPUT INSERTED.[Id]
                VALUES
                    (@Sku, @Title, @Category, @Description,
                     (@ImageUrl), @Price, @StockQuantity, @Status, @CreatedAt);
                """;

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);

            BindParameters(command, product);
            command.Parameters.Add("@CreatedAt", SqlDbType.DateTime2).Value = DateTime.UtcNow;

            await connection.OpenAsync();
            object? insertedId = await command.ExecuteScalarAsync();

            return Convert.ToInt32(insertedId);
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            const string sql = """
                UPDATE [dbo].[Products]
                SET [Sku] = @Sku,
                    [Title] = @Title,
                    [Category] = @Category,
                    [Description] = @Description,
                    [ImageUrl] = @ImageUrl,
                    [Price] = @Price,
                    [StockQuantity] = @StockQuantity,
                    [Status] = @Status
                WHERE [Id] = @Id;
                """;

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);

            BindParameters(command, product);
            command.Parameters.Add("@Id", SqlDbType.Int).Value = product.Id;

            await connection.OpenAsync();
            int rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = """
                DELETE FROM [dbo].[Products]
                WHERE [Id] = @Id;
                """;

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            await connection.OpenAsync();
            int rows = await command.ExecuteNonQueryAsync();

            return rows > 0;
        }

        public async Task<IReadOnlyList<string>> GetCategoriesAsync()
        {
            var categories = new List<string>();
            const string sql = """
                SELECT DISTINCT [Category]
                FROM [dbo].[Products]
                ORDER BY [Category] ASC;
                """;

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                if (!reader.IsDBNull(0))
                {
                    categories.Add(reader.GetString(0));
                }
            }

            return categories;
        }

        public async Task<InventorySummary> GetSummaryMetricsAsync()
        {
            const string sql = """
                SELECT 
                    COUNT(*) AS TotalCount,
                    ISNULL(SUM(CASE WHEN [Status] = 'Active' THEN 1 ELSE 0 END), 0) AS ActiveCount,
                    ISNULL(SUM(CASE WHEN [StockQuantity] <= 5 THEN 1 ELSE 0 END), 0) AS LowStockCount,
                    ISNULL(SUM([Price] * [StockQuantity]), 0.0) AS TotalValuation
                FROM [dbo].[Products];
                """;

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new InventorySummary
                {
                    TotalItems = reader.GetInt32(reader.GetOrdinal("TotalCount")),
                    ActiveProducts = reader.GetInt32(reader.GetOrdinal("ActiveCount")),
                    LowStockCount = reader.GetInt32(reader.GetOrdinal("LowStockCount")),
                    TotalValuation = reader.GetDecimal(reader.GetOrdinal("TotalValuation"))
                };
            }

            return new InventorySummary();
        }

        private static Product MapProduct(SqlDataReader reader)
        {
            return new Product
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Sku = reader.GetString(reader.GetOrdinal("Sku")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Category = reader.GetString(reader.GetOrdinal("Category")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                ImageUrl = reader.GetString(reader.GetOrdinal("ImageUrl")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                StockQuantity = reader.GetInt32(reader.GetOrdinal("StockQuantity")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }

        private static void BindParameters(SqlCommand command, Product product)
        {
            command.Parameters.Add("@Sku", SqlDbType.NVarChar, 30).Value = product.Sku;
            command.Parameters.Add("@Title", SqlDbType.NVarChar, 120).Value = product.Title;
            command.Parameters.Add("@Category", SqlDbType.NVarChar, 60).Value = product.Category;
            command.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = product.Description;
            command.Parameters.Add("@ImageUrl", SqlDbType.NVarChar, 255).Value = product.ImageUrl;
            command.Parameters.Add("@Price", SqlDbType.Decimal).Value = product.Price;
            command.Parameters.Add("@StockQuantity", SqlDbType.Int).Value = product.StockQuantity;
            command.Parameters.Add("@Status", SqlDbType.NVarChar, 40).Value = product.Status;
        }
    }
}
