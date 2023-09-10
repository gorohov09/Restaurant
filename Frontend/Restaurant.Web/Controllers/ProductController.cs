using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Restaurant.Web.Models;
using Restaurant.Web.Services.IServices;
using System.Collections.Generic;
using System.Reflection;

namespace Restaurant.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> ProductIndex()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var list = new List<ProductDto>();
            var response = await _productService.GetAllProductsAsync<ResponseDto>(accessToken);

            if (response != null && response.IsSuccess)
                list = JsonConvert.DeserializeObject<List<ProductDto>>(Convert.ToString(response.Result));

            return View(list);
        }

        public async Task<IActionResult> ProductCreate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductCreate(ProductDto model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (ModelState.IsValid)
            {
                var response = await _productService.CreateProductAsync<ResponseDto>(model, accessToken);

                if (response != null && response.IsSuccess)
                {
                    return RedirectToAction(nameof(ProductIndex));
                }
            }

            return View(model);
        }

        public async Task<IActionResult> ProductEdit(int productId)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var response = await _productService.GetProductByIdAsync<ResponseDto>(productId, accessToken);

            if (response != null && response.IsSuccess)
            {
                var model = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
                return View(model);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductEdit(ProductDto model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (ModelState.IsValid)
            {
                var response = await _productService.UpdateProductAsync<ResponseDto>(model, accessToken);

                if (response != null && response.IsSuccess)
                {
                    return RedirectToAction(nameof(ProductIndex));
                }
            }

            return View(model);
        }

        public async Task<IActionResult> ProductDelete(int productId)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var response = await _productService.DeleteProductAsync<ResponseDto>(productId, accessToken);

            if (response != null && response.IsSuccess)
            {
                return RedirectToAction(nameof(ProductIndex));
            }

            return NotFound();
        }
    }
}
