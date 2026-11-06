using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductManagement.Web.Models;
using ProductManagement.Web.Repositories;
using ProductManagement.Web.Services;

namespace ProductManagement.Web.Pages.Products
{
    public class CreateModel : PageModel
    {
        private readonly IProductRepository _repository;
        private readonly IFileStorageService _storage;

        public CreateModel(IProductRepository repository, IFileStorageService storage)
        {
            _repository = repository;
            _storage = storage;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        [BindProperty]
        public IFormFile? UploadedImage { get; set; }

        public void OnGet()
        {
            Product.Sku = $"PRD-{Random.Shared.Next(1000, 9999)}";
            Product.Category = "Peripherals";
            Product.Status = "Active";
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (UploadedImage == null || UploadedImage.Length == 0)
            {
                ModelState.AddModelError(nameof(UploadedImage), "A product photo is required for catalog registration.");
            }
            else
            {
                string? validationError = _storage.ValidateImage(UploadedImage);
                if (validationError != null)
                {
                    ModelState.AddModelError(nameof(UploadedImage), validationError);
                }
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            string savedPath = await _storage.UploadProductImageAsync(UploadedImage!, Product.Sku);

            try
            {
                Product.ImageUrl = savedPath;
                await _repository.CreateAsync(Product);

                TempData["SuccessMessage"] = $"Product '{Product.Title}' (SKU: {Product.Sku}) successfully created.";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _storage.DeleteFile(savedPath);
                ModelState.AddModelError(string.Empty, $"Data persistence failed: {ex.Message}");
                return Page();
            }
        }
    }
}
