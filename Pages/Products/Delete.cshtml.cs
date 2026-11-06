using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductManagement.Web.Models;
using ProductManagement.Web.Repositories;
using ProductManagement.Web.Services;

namespace ProductManagement.Web.Pages.Products
{
    public class DeleteModel : PageModel
    {
        private readonly IProductRepository _repository;
        private readonly IFileStorageService _storage;

        public DeleteModel(IProductRepository repository, IFileStorageService storage)
        {
            _repository = repository;
            _storage = storage;
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

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            bool deleted = await _repository.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            _storage.DeleteFile(entity.ImageUrl);

            TempData["SuccessMessage"] = $"Product '{entity.Title}' (SKU: {entity.Sku}) deleted permanently.";
            return RedirectToPage("Index");
        }
    }
}
