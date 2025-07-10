using AutoMapper;
using DAL.Entities;
using DTOs;

namespace Services.Mappers
{
    public class ProductMapper : Profile
    {
        public ProductMapper()
        {
            CreateMap<ProductDTO, Product>();
            CreateMap<NewProductDTO, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<Product, ProductDTO>();
        }
    }
}
