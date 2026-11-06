using ProductManagement.Web.Models;

namespace ProductManagement.Web.Repositories
{
    public interface IProductRepository
    {
        Task<IReadOnlyList<Product>> GetAllAsync(string? searchTerm = null, string? category = null, string? status = null);
        Task<Product?> GetByIdAsync(int id);
        Task<int> CreateAsync(Product product);
        Task<bool> UpdateAsync(Product product);
        Task<bool> DeleteAsync(int id);
        Task<IReadOnlyList<string>> GetCategoriesAsync();
        Task<InventorySummary> GetSummaryMetricsAsync();
    }
}
