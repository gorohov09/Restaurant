using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Restaurant.Services.Identity.DbContexts;
using Restaurant.Services.Identity.Models;
using System.Security.Claims;

namespace Restaurant.Services.Identity.Initializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task Initialize()
        {
            if (await _roleManager.FindByNameAsync(SD.Admin) == null)
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.Admin));
                await _roleManager.CreateAsync(new IdentityRole(SD.Customer));
            }
            else { return; }

            var adminUser = new ApplicationUser
            {
                UserName = "admin1@gmail.com",
                Email = "admin1@gmail.com",
                EmailConfirmed = true,
                PhoneNumber = "111111111",
                FirstName = "Николай",
                LastName = "Соловьев"
            };

            await _userManager.CreateAsync(adminUser, "Admin123*");
            await _userManager.AddToRoleAsync(adminUser, SD.Admin);

            var temp1 = await _userManager.AddClaimsAsync(adminUser, new Claim[]
            {
                new Claim(JwtClaimTypes.Name, adminUser.FirstName + " " + adminUser.LastName),
                new Claim(JwtClaimTypes.GivenName, adminUser.FirstName),
                new Claim(JwtClaimTypes.FamilyName, adminUser.LastName),
                new Claim(JwtClaimTypes.Role, SD.Admin),
            });

            var customerUser = new ApplicationUser
            {
                UserName = "customer1@gmail.com",
                Email = "customer1@gmail.com",
                EmailConfirmed = true,
                PhoneNumber = "111111111",
                FirstName = "Руслан",
                LastName = "Маратканов"
            };

            await _userManager.CreateAsync(customerUser, "Customer123*");
            await _userManager.AddToRoleAsync(customerUser, SD.Customer);

            var temp2 = await _userManager.AddClaimsAsync(customerUser, new Claim[]
            {
                new Claim(JwtClaimTypes.Name, customerUser.FirstName + " " + customerUser.LastName),
                new Claim(JwtClaimTypes.GivenName, customerUser.FirstName),
                new Claim(JwtClaimTypes.FamilyName, customerUser.LastName),
                new Claim(JwtClaimTypes.Role, SD.Customer),
            });
        }
    }
}
