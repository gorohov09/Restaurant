using Restaurant.Web.Models;
using Restaurant.Web.Services.IServices;
using static Restaurant.Web.SD;

namespace Restaurant.Web.Services
{
    public class OrderService : BaseService, IOrderService
    {
        public OrderService(IHttpClientFactory httpClient) 
            : base(httpClient)
        {
        }

        public async Task<T> GetAllOrdersAsync<T>(string token)
        {
            return await SendAsync<T>(new ApiRequest
            {
                AccessToken = token,
                ApiType = ApiType.GET,
                Url = SD.OrderAPIBase + "api/order/GetOrdersUser"
            });
        }
    }
}
