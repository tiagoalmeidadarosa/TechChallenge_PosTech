using TechChallenge.API.Models.Requests;
using TechChallenge.API.Validators;

namespace TechChallenge.Tests.Validators;

public class FilteredContactsRequestValidatorTests
{
    private readonly FilteredContactsRequestValidator _validator = new();

    [Fact]
    public void Validate_ShouldReturnError_WhenDDDIsLessThan11()
    {
        // Arrange
        var request = new FilteredContactsRequest { DDD = 10 };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("'DDD' must be between 11 and 99. You entered 10.", result.Errors.Single().ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenDDDIsGreaterThan99()
    {
        // Arrange
        var request = new FilteredContactsRequest { DDD = 100 };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("'DDD' must be between 11 and 99. You entered 100.", result.Errors.Single().ErrorMessage);
    }

    [Theory]
    [InlineData(11)]
    [InlineData(51)]
    [InlineData(99)]
    [InlineData(null)]
    public void Validate_ShouldReturnValid_WhenDDDIsBetween11And99OrNull(int? ddd)
    {
        // Arrange
        var request = new FilteredContactsRequest { DDD = ddd };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }
}
