using DTOs.ApiDtos;
using FluentValidation;

namespace API.Validations
{
    public class ProductValidation : AbstractValidator<ProductDTO>
    {
        public ProductValidation()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("The product id can not be empty");

            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("The product name can not be empty");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("The price must be greater than 0.");

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("The stock must be greater than or equal to 0.");
        }
    }
}
