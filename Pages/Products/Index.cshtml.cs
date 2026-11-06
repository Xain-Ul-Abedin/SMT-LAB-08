using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductManagement.Web.Models;
using ProductManagement.Web.Repositories;

namespace ProductManagement.Web.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly IProductRepository _repository;

        public IndexModel(IProductRepository repository)
        {
            _repository = repository;
        }

        public IReadOnlyList<Product> Products { get; private set; } = Array.Empty<Product>();
        public IReadOnlyList<string> Categories { get; private set; } = Array.Empty<string>();
        public InventorySummary Summary { get; private set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Query { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Category { get; set; } = "All";

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; } = "All";

        public async Task OnGetAsync()
        {
            try
            {
                Categories = await _repository.GetCategoriesAsync();
                Summary = await _repository.GetSummaryMetricsAsync();
                Products = await _repository.GetAllAsync(Query, Category, StatusFilter);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Database connectivity alert: {ex.Message}";
                Products = Array.Empty<Product>();
            }
        }
    }
}
