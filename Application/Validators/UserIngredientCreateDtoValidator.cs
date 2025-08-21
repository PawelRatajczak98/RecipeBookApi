using FluentValidation;
using Application.DTO.UserIngredient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class UserIngredientCreateDtoValidator : AbstractValidator<UserIngredientCreateDto>
    {
        public UserIngredientCreateDtoValidator()
        {
            RuleFor(x => x.IngredientId)
                .NotEmpty().WithMessage("Ingredient ID is required");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero")
                .LessThan(1000000).WithMessage("Million is too much");

            RuleFor( x=> x.Unit)
                .NotEmpty().WithMessage("Unit is required")
                .MaximumLength(50).WithMessage("Unit cannot exceed 50 characters");
        }
    }
}
