using TechChallenge_Fase01.API.Models.Requests;
using TechChallenge_Fase01.API.Validators;

namespace TechChallenge_Fase01.Tests.Validators;

public class ContactRequestValidatorTests
{
    private readonly ContactRequestValidator _validator = new();

    [Fact]
    public void Validate_ShouldReturnError_WhenDDDIsLessThan11()
    {
        // Arrange
        var request = new ContactRequest { DDD = 10 };

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
        var request = new ContactRequest { Name = "", Phone = "" };

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
        var request = new ContactRequest { Phone = "123", Email = "test" };

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
        var request = new ContactRequest
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
