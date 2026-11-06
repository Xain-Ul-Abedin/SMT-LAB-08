using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductManagement.Web.Models;
using ProductManagement.Web.Repositories;

namespace ProductManagement.Web.Pages.Products
{
    public class DetailsModel : PageModel
    {
        private readonly IProductRepository _repository;

        public DetailsModel(IProductRepository repository)
        {
            _repository = repository;
        }

        public Product Product { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            Product = entity;
            return Page();
        }
    }
}
