using Application.DTO.RecipeIngredient;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class RecipeIngredientDtoValidator : AbstractValidator<RecipeIngredientDto>
    {
        public RecipeIngredientDtoValidator()
        {
            RuleFor(x => x.IngredientName)
                .NotEmpty().WithMessage("Name of ingredient is required");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero");

            RuleFor(x => x.Unit)
                .NotEmpty().WithMessage("Unit of measurement is required");
        }
    }
}
