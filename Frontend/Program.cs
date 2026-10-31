using Microsoft.AspNetCore.OutputCaching;
using CatalogManager.Services;
using CatalogManager.Hubs;
using CatalogManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CatalogManager.Application.Interfaces;
using CatalogManager.Application.Services;
using CatalogManager.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// MVC and API
builder.Services.AddControllersWithViews();

// B9: Output Cache
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("CategoriesCache", policy =>
        policy.Expire(TimeSpan.FromSeconds(30))
              .Tag("categories"));

    options.AddPolicy("StoresCache", policy =>
        policy.Expire(TimeSpan.FromSeconds(30))
              .Tag("stores"));

    options.AddPolicy("ProductsCache", policy =>
        policy.Expire(TimeSpan.FromSeconds(30))
              .Tag("products"));
});

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Categories
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// Stores
builder.Services.AddScoped<IStoreRepository, StoreRepository>();
builder.Services.AddScoped<IStoreService, StoreService>();

// Products
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

// B4: Lucene.NET full-text product search
builder.Services.AddScoped<ProductSearchService>();

builder.Services.AddHttpClient<
    ICurrencyRatesService,
    PrivatBankCurrencyRatesService>(client =>
    {
        client.BaseAddress = new Uri("https://api.privatbank.ua/");
        client.Timeout = TimeSpan.FromSeconds(10);
    });

builder.Services.AddSingleton<SharedNotesStore>();
builder.Services.AddSignalR();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// B9: Enable Output Cache middleware
app.UseOutputCache();

app.UseAuthorization();

app.MapStaticAssets();

// B4: enable attribute-routed API endpoints
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHub<NotesHub>("/notesHub");

app.Run();

public partial class Program { }