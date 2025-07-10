using DTOs;
using FluentValidation;

namespace API.Validations
{
    public class NewProductValidation : AbstractValidator<ProductInputDTO>
    {
        public NewProductValidation()
        {
            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("The product name can not be empty");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("The price must be greater than 0.");

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("The stock must be greater than or equal to 0.");
        }
    }
}
