using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductManagement.Web.Models;
using ProductManagement.Web.Repositories;
using ProductManagement.Web.Services;

namespace ProductManagement.Web.Pages.Products
{
    public class EditModel : PageModel
    {
        private readonly IProductRepository _repository;
        private readonly IFileStorageService _storage;

        public EditModel(IProductRepository repository, IFileStorageService storage)
        {
            _repository = repository;
            _storage = storage;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        [BindProperty]
        public IFormFile? ReplacementImage { get; set; }

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
            if (id != Product.Id)
            {
                return BadRequest();
            }

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            if (ReplacementImage != null && ReplacementImage.Length > 0)
            {
                string? validationError = _storage.ValidateImage(ReplacementImage);
                if (validationError != null)
                {
                    ModelState.AddModelError(nameof(ReplacementImage), validationError);
                }
            }

            if (!ModelState.IsValid)
            {
                Product.ImageUrl = existing.ImageUrl;
                return Page();
            }

            string oldPath = existing.ImageUrl;
            string finalPath = oldPath;

            if (ReplacementImage != null && ReplacementImage.Length > 0)
            {
                finalPath = await _storage.UploadProductImageAsync(ReplacementImage, Product.Sku);
            }

            Product.ImageUrl = finalPath;

            try
            {
                bool success = await _repository.UpdateAsync(Product);
                if (!success)
                {
                    if (finalPath != oldPath)
                    {
                        _storage.DeleteFile(finalPath);
                    }
                    return NotFound();
                }

                if (finalPath != oldPath)
                {
                    _storage.DeleteFile(oldPath);
                }

                TempData["SuccessMessage"] = $"Product '{Product.Title}' updated successfully.";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                if (finalPath != oldPath)
                {
                    _storage.DeleteFile(finalPath);
                }
                ModelState.AddModelError(string.Empty, $"Update failed: {ex.Message}");
                return Page();
            }
        }
    }
}
