using ProductManagement.Web.Repositories;
using ProductManagement.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Pages presentation framework
builder.Services.AddRazorPages();

// Register decoupled repository and storage service layers
builder.Services.AddScoped<IProductRepository, SqlProductRepository>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
