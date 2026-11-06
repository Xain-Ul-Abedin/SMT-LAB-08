namespace ProductManagement.Web.Models
{
    public class InventorySummary
    {
        public int TotalItems { get; set; }
        public int ActiveProducts { get; set; }
        public int LowStockCount { get; set; }
        public decimal TotalValuation { get; set; }
    }
}
