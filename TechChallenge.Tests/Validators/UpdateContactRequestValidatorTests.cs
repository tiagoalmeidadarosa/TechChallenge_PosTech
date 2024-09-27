using TechChallenge.API.Update.Models.Requests;
using TechChallenge.API.Update.Validators;

namespace TechChallenge.Tests.Validators;

public class UpdateContactRequestValidatorTests
{
    private readonly UpdateContactRequestValidator _validator = new();

    [Fact]
    public void Validate_ShouldReturnError_WhenDDDIsLessThan11()
    {
        // Arrange
        var request = new UpdateContactRequest { DDD = 10 };

        // Act
        var result = _validator.Validate(request);

        var messages = result.Errors.Select(e => e.ErrorMessage).ToList();

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("'DDD' must be between 11 and 99. You entered 10.", messages);
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenNameOrPhoneOrEmailAreEmpty()
    {
        // Arrange
        var request = new UpdateContactRequest { Name = "", Phone = "" };

        // Act
        var result = _validator.Validate(request);
        var messages = result.Errors.Select(e => e.ErrorMessage).ToList();

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Name is required.", messages);
        Assert.Contains("Phone is required.", messages);
        Assert.Contains("Email is required.", messages);
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenPhoneOrEmailIsInvalid()
    {
        // Arrange
        var request = new UpdateContactRequest { Phone = "123", Email = "test" };

        // Act
        var result = _validator.Validate(request);
        var messages = result.Errors.Select(e => e.ErrorMessage).ToList();

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Invalid phone number.", messages);
        Assert.Contains("Invalid email address.", messages);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess()
    {
        // Arrange
        var request = new UpdateContactRequest
        {
            DDD = 11,
            Name = "Test",
            Phone = "123456789",
            Email = "test@test.net"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }
}
