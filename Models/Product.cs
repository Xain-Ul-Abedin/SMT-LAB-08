using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Web.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "SKU is required.")]
        [StringLength(30, ErrorMessage = "SKU code cannot exceed 30 characters.")]
        [Display(Name = "Stock Keeping Unit (SKU)")]
        public string Sku { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product title is required.")]
        [StringLength(120, ErrorMessage = "Title cannot exceed 120 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        [StringLength(60)]
        public string Category { get; set; } = "Hardware";

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;

        [StringLength(255)]
        [Display(Name = "Product Image")]
        public string ImageUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 100000.00, ErrorMessage = "Price must be between 0.01 and 100,000.00.")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0, 100000, ErrorMessage = "Stock quantity cannot be negative.")]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Inventory status is required.")]
        [StringLength(40)]
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
