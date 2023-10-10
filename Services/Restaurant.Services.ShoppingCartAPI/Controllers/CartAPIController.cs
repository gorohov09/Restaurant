using Microsoft.AspNetCore.Mvc;
using Restaurant.Services.ShoppingCartAPI.Messages;
using Restaurant.Services.ShoppingCartAPI.Models.Dto;
using Restaurant.Services.ShoppingCartAPI.RabbitMQSender;
using Restaurant.Services.ShoppingCartAPI.Repository;

namespace Restaurant.Services.ShoppingCartAPI.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartAPIController : Controller
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICouponRepository _couponRepository;
        private readonly IRabbitMQCartMessageSender _rabbitMessageSender;
        protected ResponseDto _response;

        public CartAPIController(
            ICartRepository cartRepository,
            ICouponRepository couponRepository,
            IRabbitMQCartMessageSender rabbitMessageSender)
        {
            _cartRepository = cartRepository;
            _rabbitMessageSender = rabbitMessageSender;
            _response = new ResponseDto();
            _couponRepository = couponRepository;
        }

        [HttpGet("GetCart/{userId}")]
        public async Task<object> GetCart(string userId)
        {
            try
            {
                CartDto cartDto = await _cartRepository.GetCartByUserId(userId);
                _response.Result = cartDto;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPost("AddCart")]
        public async Task<object> AddCart([FromBody] CartDto cartDto)
        {
            try
            {
                CartDto cartDt = await _cartRepository.CreateUpdateCart(cartDto);
                _response.Result = cartDt;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPost("UpdateCart")]
        public async Task<object> UpdateCart([FromBody]CartDto cartDto)
        {
            try
            {
                CartDto cartDt = await _cartRepository.CreateUpdateCart(cartDto);
                _response.Result = cartDt;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPost("RemoveCart")]
        public async Task<object> RemoveCart([FromBody] int cartId)
        {
            try
            {
                bool isSuccess = await _cartRepository.RemoveFromCart(cartId);
                _response.Result = isSuccess;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPost("ApplyCoupon")]
        public async Task<object> ApplyCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                bool isSuccess = await _cartRepository.ApplyCoupon(cartDto.CartHeader.UserId,
                    cartDto.CartHeader.CouponCode);
                _response.Result = isSuccess;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPost("RemoveCoupon")]
        public async Task<object> RemoveCoupon([FromBody] string userId)
        {
            try
            {
                bool isSuccess = await _cartRepository.RemoveCoupon(userId);
                _response.Result = isSuccess;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPost("Checkout")]
        public async Task<object> Checkout(CheckoutHeaderDto checkoutHeader)
        {
            /* Логика здесь следующая: ShoppinCartAPI получает информацию о заказе + актуальную корзину пользователя
             * И отправляет это сообщение в сервис заказов - OrderAPI. Отправка происходит через брокер сообщений.
             */

            try
            {
                var cartDto = await _cartRepository.GetCartByUserId(checkoutHeader.UserId);
                if (cartDto == null)
                {
                    return BadRequest();
                }
                checkoutHeader.CartDetails = cartDto.CartDetails;

                //Проверим, а действует ли скидка(Возможно купон был изменен).
                //Для этого нам необходимо обратиться в сервис купонов. И обращение это будет синхронным
                if (!string.IsNullOrEmpty(checkoutHeader.CouponCode))
                {
                    var coupon = await _couponRepository.GetCoupon(checkoutHeader.CouponCode);

                    if (coupon.DiscountAmount != checkoutHeader.DiscountTotal)
                    {
                        _response.IsSuccess = false;
                        _response.ErrorMessages = new List<string>() { "Купон изменился" };
                        _response.DisplayMessage = "Купон изменился";
                        return _response;
                    }
                }

                //Логика для передачи сообщения в сервис обработки заказа
                _rabbitMessageSender.SendMessage(checkoutHeader, "checkoutqueue");

                //Очистка корзины
                await _cartRepository.ClearCart(checkoutHeader.UserId);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return _response;
        }
    }
}
