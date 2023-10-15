using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Restaurant.Services.ProductAPI.DbContexts;
using Restaurant.Services.ProductAPI.Models;
using Restaurant.Services.ProductAPI.Models.Dto;

namespace Restaurant.Services.ProductAPI.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public ProductRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ProductDto> CreateUpdateProduct(ProductDto productDto)
        {
            var product = _mapper.Map<Product>(productDto);

            if (product.ProductId > 0) //Если true - то такой товар уже был создан
                _db.Products.Update(product);
            else
                _db.Products.Add(product);

            await _db.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<bool> DeleteProduct(int productId)
        {
            try
            {
                var product = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == productId);

                if (product is null)
                    return false;

                _db.Products.Remove(product);   
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<FindIdsUndiscoveredProductsDto> FindIdsUndiscoveredProducts(int[] productsIds)
        {
            var result = new List<int>();
            foreach (var productId in productsIds)
            {
                if (!await _db.Products.AnyAsync(p => p.ProductId == productId))
                    result.Add(productId);
            }

            return new FindIdsUndiscoveredProductsDto
            {
                ProductsIds = result.ToArray()
            };
        }

        public async Task<ProductDto> GetProductById(int productId)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<IEnumerable<ProductDto>> GetProducts()
        {
            var productList = await _db.Products.ToListAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(productList);
        }
    }
}
