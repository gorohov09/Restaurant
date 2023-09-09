using Restaurant.Web;
using Restaurant.Web.Services;
using Restaurant.Web.Services.IServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;

services.AddControllersWithViews();
services.AddHttpClient<IProductService, ProductService>();
SD.ProductAPIBase = builder.Configuration["ServiceUrls:ProductAPI"];

services.AddScoped<IProductService, ProductService>();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
