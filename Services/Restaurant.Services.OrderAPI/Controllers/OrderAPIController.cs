using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Services.OrderAPI.Abstractions;
using Restaurant.Services.OrderAPI.Models.Dto;
using Restaurant.Services.OrderAPI.Repository;

namespace Restaurant.Services.OrderAPI.Controllers
{
    [ApiController]
    [Route("api/order")]
    public class OrderAPIController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        protected ResponseDto _response;

        public OrderAPIController(IOrderRepository orderRepository, IUserContext userContext, IMapper mapper)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _userContext = userContext;
            _response = new ResponseDto();
        }

        [Authorize]
        [HttpGet("GetOrdersUser")]
        public async Task<object> GetOrders()
        {
            var userId = _userContext.CurrentUserId;

            try
            {
                var orderDtos = _mapper.Map<List<OrderDto>>(await _orderRepository.GetAllOrdersByUser(userId));
                _response.Result = orderDtos;
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
