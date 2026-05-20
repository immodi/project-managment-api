using Application.DTOs.Auth;
using Application.Validators;
using FluentAssertions;
using FluentValidation;
using Xunit;

namespace Tests.Validators;

public class RegisterRequestValidatorTests
{
    // NOTE: Adjust this if your actual validator class has a different namespace/name
    private readonly IValidator<RegisterRequest> _validator = new RegisterRequestValidator();

    [Fact]
    public void Should_pass_for_valid_request()
    {
        var request = new RegisterRequest
        {
            Email = "valid@test.com",
            Password = "Password123!"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_fail_when_email_is_empty()
    {
        var request = new RegisterRequest
        {
            Email = "",
            Password = "Password123!"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Should_fail_when_email_is_invalid()
    {
        var request = new RegisterRequest
        {
            Email = "not-an-email",
            Password = "Password123!"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Should_fail_when_password_is_empty()
    {
        var request = new RegisterRequest
        {
            Email = "valid@test.com",
            Password = ""
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Should_fail_when_password_is_too_short()
    {
        var request = new RegisterRequest
        {
            Email = "valid@test.com",
            Password = "123"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}