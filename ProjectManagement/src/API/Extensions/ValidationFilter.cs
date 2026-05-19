using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Extensions;

public class ValidationFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments)
        {
            if (argument.Value is null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.Value.GetType());
            var validator = serviceProvider.GetService(validatorType);

            if (validator == null)
                continue;

            var method = validatorType.GetMethod("ValidateAsync")!;
            var task = (Task)method.Invoke(validator, [argument.Value, CancellationToken.None])!;
            await task.ConfigureAwait(false);

            var resultProperty = task.GetType().GetProperty("Result")!;
            var result = (FluentValidation.Results.ValidationResult)resultProperty.GetValue(task)!;

            if (!result.IsValid)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    error = "Validation failed",
                    details = result.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        message = e.ErrorMessage
                    })
                });

                return;
            }
        }

        await next();
    }
}