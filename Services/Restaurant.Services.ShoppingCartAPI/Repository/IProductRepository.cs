namespace Restaurant.Services.ShoppingCartAPI.Repository
{
    public interface IProductRepository
    {
        /// <summary>
        /// Получение Id-s ненайденных товаров
        /// </summary>
        /// <param name="productsIds">Список Id-s всех товаров</param>
        /// <returns>Список Id-s ненайденных товаров</returns>
        Task<List<int>> FindIdsUndiscoveredProducts(int[] productsIds);
    }
}
