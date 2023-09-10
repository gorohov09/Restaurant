using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Restaurant.Services.Identity;
using Restaurant.Services.Identity.DbContexts;
using Restaurant.Services.Identity.Initializer;
using Restaurant.Services.Identity.Models;
using Restaurant.Services.Identity.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;
services.AddControllersWithViews();

services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

var builderIdentityServer = services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;
    options.EmitStaticAudienceClaim = true;
}).AddInMemoryIdentityResources(SD.IdentityResources)
            .AddInMemoryApiScopes(SD.ApiScopes)
            .AddInMemoryClients(SD.Clients)
            .AddAspNetIdentity<ApplicationUser>();

services.AddScoped<IDbInitializer, DbInitializer>();
services.AddScoped<IProfileService, ProfileService>();

builderIdentityServer.AddDeveloperSigningCredential();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseIdentityServer();
app.UseAuthorization();

using ServiceProvider serviceProvider = services.BuildServiceProvider();
using var scope = serviceProvider.CreateScope(); // контекст scope
var dbInitializer = scope.ServiceProvider.GetService<IDbInitializer>();
await dbInitializer.Initialize();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
