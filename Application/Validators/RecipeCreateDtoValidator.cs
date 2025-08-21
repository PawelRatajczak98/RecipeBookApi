using Application.DTO.Recipe;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class RecipeCreateDtoValidator : AbstractValidator<RecipeCreateDto>
    {
        public RecipeCreateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Recipe name is required.")
                .MaximumLength(100).WithMessage("Recipe name cannot exceed 100 characters.");
            
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Recipe description is required.")
                .MaximumLength(500).WithMessage("Recipe description cannot exceed 500 characters.");
            
            RuleFor(x => x.RecipeIngredientsDto)
                .NotEmpty().WithMessage("At least one ingredient is required.")
                .Must(ingredients => ingredients.All(i => i.IngredientId > 0 && i.Quantity > 0))
                .WithMessage("Each ingredient must have a valid ID and quantity greater than zero.");

        }
    }
}
