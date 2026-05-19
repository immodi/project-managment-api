using Application.DTOs.Tasks;
using FluentValidation;

namespace Application.Validators;

public class CreateTaskValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required")
            .MaximumLength(150).WithMessage("Task title must not exceed 150 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => x.Description != null);

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Due date must be in the future");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required");
    }
}