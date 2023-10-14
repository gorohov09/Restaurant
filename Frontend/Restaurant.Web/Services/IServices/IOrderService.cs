namespace Restaurant.Web.Services.IServices
{
    public interface IOrderService
    {
        Task<T> GetAllOrdersAsync<T>(string token);
    }
}
