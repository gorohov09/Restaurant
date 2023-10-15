using Newtonsoft.Json;
using Restaurant.Services.ShoppingCartAPI.Models.Dto;

namespace Restaurant.Services.ShoppingCartAPI.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly HttpClient _client;

        public ProductRepository(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<int>> FindIdsUndiscoveredProducts(int[] productsIds)
        {
            var response = await _client.PostAsJsonAsync(
                $"/api/products/FindIdsUndiscoveredProducts", 
                new FindIdsUndiscoveredProductsDto { ProductsIds = productsIds});

            var apiContent = await response.Content.ReadAsStringAsync();
            var resp = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
            if (resp != null && resp.IsSuccess)
            {
                var productIds = JsonConvert.DeserializeObject<FindIdsUndiscoveredProductsDto>(Convert.ToString(resp.Result));
                return productIds?.ProductsIds?.Length > 0 ? productIds.ProductsIds.ToList() : new List<int>();
            }

            return new List<int>();
        }
    }

    public class FindIdsUndiscoveredProductsDto
    {
        public int[] ProductsIds { get; set; }
    }
}
