using AutoMapper;
using DAL.Entities;
using DTOs.ApiDtos;

namespace Services.Mappers
{
    public class ProductMapper : Profile
    {
        public ProductMapper()
        {
            CreateMap<ProductDTO, Product>();
            CreateMap<ProductInputDTO, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<Product, ProductDTO>();
        }
    }
}
