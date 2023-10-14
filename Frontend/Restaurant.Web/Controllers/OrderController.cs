using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Restaurant.Web.Models;
using Restaurant.Web.Services.IServices;

namespace Restaurant.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> OrderIndex()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var list = new List<OrderDto>();
            var response = await _orderService.GetAllOrdersAsync<ResponseDto>(accessToken);

            if (response != null && response.IsSuccess)
                list = JsonConvert.DeserializeObject<List<OrderDto>>(Convert.ToString(response.Result));

            var result = list.Select(o => new OrderVm
            {
                DiscountTotal = o.DiscountTotal,
                PickupDateTime = o.PickupDateTime,
                CartTotalItems = o.CartTotalItems,
                CouponCode = o.CouponCode,
                Email = o.Email,
                OrderId = o.OrderId,
                OrderTime = o.OrderTime,
                OrderTotal = o.OrderTotal,
                Phone = o.Phone,
                PaymentStatus = o.PaymentStatus ? "Оплачено" : "В процессе оплаты"
            });

            return View(result);
        }
    }
}
