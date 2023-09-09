using Restaurant.Web.Models;
using Restaurant.Web.Services.IServices;

namespace Restaurant.Web.Services
{
    public class ProductService : BaseService, IProductService
    {
        public ProductService(IHttpClientFactory httpClient) : base(httpClient)
        {
        }

        public async Task<T> CreateProductAsync<T>(ProductDto productDto)
        {
            return await SendAsync<T>(new ApiRequest
            {
                Data = productDto,
                ApiType = SD.ApiType.POST,
                Url = SD.ProductAPIBase + "api/products",
                AccessToken = ""
            });
        }

        public async Task<T> DeleteProductAsync<T>(int id)
        {
            return await SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.DELETE,
                Url = SD.ProductAPIBase + "api/products/" + id,
                AccessToken = ""
            });
        }

        public async Task<T> GetAllProductsAsync<T>()
        {
            return await SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = SD.ProductAPIBase + "api/products/",
                AccessToken = ""
            });
        }

        public async Task<T> GetProductByIdAsync<T>(int id)
        {
            return await SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = SD.ProductAPIBase + "api/products/" + id,
                AccessToken = ""
            });
        }

        public async Task<T> UpdateProductAsync<T>(ProductDto productDto)
        {
            return await SendAsync<T>(new ApiRequest
            {
                Data = productDto,
                ApiType = SD.ApiType.PUT,
                Url = SD.ProductAPIBase + "api/products",
                AccessToken = ""
            });
        }
    }
}
