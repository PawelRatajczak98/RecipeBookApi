using Application.DTO.Ingredient;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class IngredientCreateDtoValidator : AbstractValidator<IngredientCreateDto>
    {
        public IngredientCreateDtoValidator()
        {
            RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(23);

            RuleFor(x => x.Description)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(50);

            RuleFor(x => x.PriceFor100Grams)
                .NotEmpty()
                .GreaterThan(0);
        }
    }

}

