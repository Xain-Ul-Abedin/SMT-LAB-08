# Enterprise Product Management System (PMS)

A production-ready Product Management System built with ASP.NET Core Razor Pages, asynchronous ADO.NET (`Microsoft.Data.SqlClient`), and an interface-driven Repository Design Pattern.

---

## Architecture Specification

This solution decouples data persistence from the presentation layer through distinct interfaces and asynchronous execution pipelines:

* **Presentation Layer (`Pages/Products/`)**:
  * `Index`: Real-time inventory catalogue featuring KPI metric cards (Total items, Active products, Low stock warnings, Total inventory valuation), dynamic category filtering, search query handling, and status badges.
  * `Create`: Product registration form with automated SKU seed generation, multipart image uploading, and client/server validation.
  * `Edit`: Live metadata updating with optional image replacement and orphaned asset deletion.
  * `Details`: Single-product analytical view with high-resolution image inspection and valuation breakdown.
  * `Delete`: Safety confirmation dialog with complete physical asset cleanup.

* **Repository Layer (`Repositories/`)**:
  * `IProductRepository`: Contract defining data access methods (`GetAllAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `GetCategoriesAsync`, `GetSummaryMetricsAsync`).
  * `SqlProductRepository`: Concrete implementation utilizing parameterized SQL commands via `Microsoft.Data.SqlClient`, preventing SQL injection and maximizing query throughput.

* **Storage Service Layer (`Services/`)**:
  * `IFileStorageService` & `LocalFileStorageService`: Enforces 8MB file bounds, whitelists modern image extensions (`.jpg`, `.jpeg`, `.png`, `.webp`, `.svg`), generates timestamped unique filenames, and guards against directory traversal vulnerabilities.

* **Domain Models (`Models/`)**:
  * `Product.cs`: Encapsulates entity state, SKU uniqueness, and validation rules.
  * `InventorySummary.cs`: Aggregates real-time business telemetry metrics.

---

## Database Provisioning

1. Connect to your SQL Server or LocalDB instance.
2. Execute the script at `Database/init_inventory_db.sql` to initialize `SMT_Inventory_DB` and create the `Products` table with sample records.
3. Configure your connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SMT_Inventory_DB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

---

## Execution Guide

### Prerequisites
* .NET 9.0 SDK
* Microsoft SQL Server / LocalDB

### Terminal Commands

```bash
# Restore NuGet dependencies
dotnet restore

# Build project assembly
dotnet build

# Launch application
dotnet run
```

Access the portal in your browser at `https://localhost:5001` or `http://localhost:5000`. Navigate to `/Products` to access the full inventory management workbench.
