using AutoMapper;
using Restaurant.Services.OrderAPI.Models;
using Restaurant.Services.OrderAPI.Models.Dto;

namespace Restaurant.Services.OrderAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<OrderHeader, OrderDto>()
                    .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderHeaderId))
                    .ReverseMap();

                config.CreateMap<OrderDetails, OrderItemDto>()
                    .ForMember(dest => dest.OrderItemId, opt => opt.MapFrom(src => src.OrderDetailsId))
                    .ReverseMap();
            });

            return mappingConfig;
        }
    }
}
