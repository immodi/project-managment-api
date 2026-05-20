using Application.DTOs.Projects;
using Application.Validators;
using FluentAssertions;
using FluentValidation;
using Xunit;

namespace Tests.Validators;

public class CreateProjectValidatorTests
{
    private readonly IValidator<CreateProjectRequest> _validator = new CreateProjectValidator();

    [Fact]
    public void Should_pass_for_valid_request()
    {
        var request = new CreateProjectRequest
        {
            Name = "Valid Project",
            Description = "Valid description"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_fail_when_name_is_empty()
    {
        var request = new CreateProjectRequest
        {
            Name = "",
            Description = "Desc"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Should_fail_when_name_is_too_long()
    {
        var request = new CreateProjectRequest
        {
            Name = new string('A', 101),
            Description = "Desc"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Should_fail_when_description_is_too_long()
    {
        var request = new CreateProjectRequest
        {
            Name = "Valid Name",
            Description = new string('B', 501)
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Fact]
    public void Should_pass_when_description_is_null()
    {
        var request = new CreateProjectRequest
        {
            Name = "Valid Name",
            Description = null
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}