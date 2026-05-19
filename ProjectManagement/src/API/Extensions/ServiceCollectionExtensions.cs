using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<ValidationFilter>();
        });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        services.AddValidatorsFromAssemblyContaining<
            Application.Validators.RegisterRequestValidator>();

        return services;
    }
}