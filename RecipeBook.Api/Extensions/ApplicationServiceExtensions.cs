using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Services;
using System.Text.Json.Serialization;
using Application.Validators;
using FluentValidation;
using System.Reflection;

namespace Api.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services,
            IConfiguration config)
        {

            services.AddValidatorsFromAssemblyContaining<RecipeCreateDtoValidator>();

            services.AddControllers()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions
                    .ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

            services.AddHttpContextAccessor();
            services.AddEndpointsApiExplorer();
            

            

            services.AddCors();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IIngredientService, IngredientService>();
            services.AddScoped<Random>();
            services.AddScoped<IRecipeGeneratorService, RecipeGeneratorService>();
            services.AddScoped<IRecipeService, RecipeService>();
            services.AddScoped<IUserIngredientService, UserIngredientService>();
            services.AddScoped<IUserContextService, UserContextService>();
            services.AddScoped<IBudgetService, BudgetService>();
            services.AddScoped<IRecipeFeedbackService, RecipeFeedbackService>();
            return services;
        }
    }
}
