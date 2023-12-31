﻿using Microsoft.AspNetCore.Mvc;
using Restaurant.Services.ShoppingCartAPI.Messages;
using Restaurant.Services.ShoppingCartAPI.Models.Dto;
using Restaurant.Services.ShoppingCartAPI.RabbitMQSender;
using Restaurant.Services.ShoppingCartAPI.Repository;
using System.Security.Claims;

namespace Restaurant.Services.ShoppingCartAPI.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartAPIController : Controller
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICouponRepository _couponRepository;
        private readonly IProductRepository _productRepository;
        private readonly IRabbitMQCartMessageSender _rabbitMessageSender;
        protected ResponseDto _response;

        public CartAPIController(
            ICartRepository cartRepository,
            ICouponRepository couponRepository,
            IRabbitMQCartMessageSender rabbitMessageSender,
            IProductRepository productRepository)
        {
            _cartRepository = cartRepository;
            _rabbitMessageSender = rabbitMessageSender;
            _response = new ResponseDto();
            _couponRepository = couponRepository;
            _productRepository = productRepository;
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

                //Проверим, а существуют ли товары, которые заказывают
                //Логика: отправляем список Id товаров в ProductAPI, который возвразает также список Id ненайденных товаров
                //Проходимся по списку Id ненайденных товаров и удаляем их из корзины юзера
                if (checkoutHeader.CartDetails?.Count() > 0)
                {
                    //Поиск из корзины список id товаров, которые заказали
                    var listProductIds = checkoutHeader.CartDetails
                        .Select(x => x.ProductId)
                        .ToArray();

                    //Поиск id товаров, которых не существует(удалили, пока юзер заполнял данные по заказу - условно)
                    var listUndiscoverProductIds = await _productRepository.FindIdsUndiscoveredProducts(listProductIds);

                    if (listUndiscoverProductIds.Any())
                    {
                        //Поиск по id ненайленным товарам - id элементов корзины. В последующим их удаление из корзины
                        var listCartDetailsIds = checkoutHeader.CartDetails
                            .Where(cartDetails => listUndiscoverProductIds.Contains(cartDetails.ProductId))
                            .Select(cartDetails => cartDetails.CartDetailsId)
                            .ToArray();

                        foreach (var cartDetailsId in listCartDetailsIds)
                            await _cartRepository.RemoveFromCart(cartDetailsId);

                        _response.IsSuccess = false;
                        _response.ErrorMessages = new List<string>() { "Некоторые товары больше недоступны" };
                        _response.DisplayMessage = "Некоторые товары больше недоступны";
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
