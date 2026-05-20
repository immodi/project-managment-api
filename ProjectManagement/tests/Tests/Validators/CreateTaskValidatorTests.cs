using Application.DTOs.Tasks;
using Application.Validators;
using FluentAssertions;
using FluentValidation;
using Domain.Enums;
using Xunit;

namespace Tests.Validators;

public class CreateTaskValidatorTests
{
    private readonly IValidator<CreateTaskRequest> _validator = new CreateTaskValidator();

    [Fact]
    public void Should_pass_for_valid_request()
    {
        var request = new CreateTaskRequest
        {
            Title = "Valid Task",
            Description = "Some description",
            DueDate = DateTime.UtcNow.AddDays(2),
            Priority = TaskPriority.High,
            ProjectId = Guid.NewGuid()
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_fail_when_title_is_empty()
    {
        var request = new CreateTaskRequest
        {
            Title = "",
            Description = "Desc",
            DueDate = DateTime.UtcNow.AddDays(2),
            Priority = TaskPriority.Medium,
            ProjectId = Guid.NewGuid()
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void Should_fail_when_due_date_is_in_past()
    {
        var request = new CreateTaskRequest
        {
            Title = "Task",
            Description = "Desc",
            DueDate = DateTime.UtcNow.AddDays(-1),
            Priority = TaskPriority.Medium,
            ProjectId = Guid.NewGuid()
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DueDate");
    }

    [Fact]
    public void Should_fail_when_project_id_is_empty()
    {
        var request = new CreateTaskRequest
        {
            Title = "Task",
            Description = "Desc",
            DueDate = DateTime.UtcNow.AddDays(2),
            Priority = TaskPriority.Medium,
            ProjectId = Guid.Empty
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProjectId");
    }
}