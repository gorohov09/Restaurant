using Microsoft.AspNetCore.Mvc;
using Restaurant.Services.CouponAPI.Models.Dto;
using Restaurant.Services.CouponAPI.Repository;

namespace Restaurant.Services.CouponAPI.Controllers
{
    [ApiController]
    [Route("api/coupon")]
    public class CouponAPIController : Controller
    {
        private readonly ICouponRepository _couponRepository;
        protected ResponseDto _response;

        public CouponAPIController(ICouponRepository couponRepository)
        {
            _couponRepository = couponRepository;
            _response = new ResponseDto();
        }

        [HttpGet("GetCoupon/{couponCode}")]
        public async Task<object> GetCart(string couponCode)
        {
            try
            {
                var couponDto = await _couponRepository.GetCouponByCode(couponCode);
                _response.Result = couponDto;
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
