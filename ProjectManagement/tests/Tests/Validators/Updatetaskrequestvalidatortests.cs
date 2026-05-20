using Application.DTOs.Tasks;
using Application.Validators;
using Domain.Enums;
using FluentAssertions;
using FluentValidation;
using Xunit;

namespace Tests.Validators;

public class UpdateTaskRequestValidatorTests
{
    private readonly IValidator<UpdateTaskRequest> _validator = new UpdateTaskRequestValidator();

    private static UpdateTaskRequest ValidRequest() => new()
    {
        Title = "Valid Title",
        Description = "Valid description",
        DueDate = DateTime.UtcNow.AddDays(2),
        Priority = TaskPriority.Medium
    };

    [Fact]
    public void Should_pass_for_valid_request()
    {
        var result = _validator.Validate(ValidRequest());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_fail_when_title_is_empty()
    {
        var request = ValidRequest();
        request.Title = "";

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void Should_fail_when_title_exceeds_max_length()
    {
        var request = ValidRequest();
        request.Title = new string('A', 151);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void Should_fail_when_description_exceeds_max_length()
    {
        var request = ValidRequest();
        request.Description = new string('B', 1001);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Fact]
    public void Should_pass_when_description_is_null()
    {
        var request = ValidRequest();
        request.Description = null;

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_fail_when_due_date_is_in_the_past()
    {
        var request = ValidRequest();
        request.DueDate = DateTime.UtcNow.AddDays(-1);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DueDate");
    }

    [Fact]
    public void Should_fail_when_priority_is_invalid_enum_value()
    {
        var request = ValidRequest();
        request.Priority = (TaskPriority)99;

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Priority");
    }

    [Theory]
    [InlineData(TaskPriority.Low)]
    [InlineData(TaskPriority.Medium)]
    [InlineData(TaskPriority.High)]
    public void Should_pass_for_all_valid_priority_values(TaskPriority priority)
    {
        var request = ValidRequest();
        request.Priority = priority;

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}