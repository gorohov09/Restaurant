using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Restaurant.Web.Models;
using Restaurant.Web.Services.IServices;
using System.Diagnostics;

namespace Restaurant.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;

        public HomeController(ILogger<HomeController> logger, IProductService productService)
        {
            _logger = logger;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            List<ProductDto> productsDto = new();
            var response = await _productService.GetAllProductsAsync<ResponseDto>("");

            if (response != null && response.IsSuccess)
                productsDto = JsonConvert.DeserializeObject<List<ProductDto>>(Convert.ToString(response.Result));

            return View(productsDto);
        }

        [Authorize]
        public async Task<IActionResult> Details(int productId)
        {
            ProductDto productDto = new();
            var response = await _productService.GetProductByIdAsync<ResponseDto>(productId, "");

            if (response != null && response.IsSuccess)
                productDto = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));

            return View(productDto);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        public async Task<IActionResult> Login()
        {
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Logout()
        {
            return SignOut("Cookies", "oidc");
        }
    }
}