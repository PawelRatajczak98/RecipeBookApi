using Application.DTO.UserIngredient;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class UserIngredientUpdateDtoValidator : AbstractValidator<UserIngredientUpdateDto>
    {
        public UserIngredientUpdateDtoValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity must be greater than or equal to zero")
                .LessThan(1000000).WithMessage("Quantity cannot exceed 1 million");

        }
    }
}
