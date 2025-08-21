using Application.Query;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class RecipeQueryValidator : AbstractValidator<RecipeQuery>
    {
        private int[] allowedPageSizes = new[] { 5, 10, 15, 20, 25 };
        public RecipeQueryValidator()
        {
            RuleFor(r => r.PageNumber).GreaterThanOrEqualTo(1);
            RuleFor(r => r.PageSize).Custom((value, context) =>
            {
                if (!allowedPageSizes.Contains(value))
                {
                    context.AddFailure("Page Size", $"PageSize must in [{string.Join(",", allowedPageSizes)}]");
                }
            
            });
        }
    }
}
