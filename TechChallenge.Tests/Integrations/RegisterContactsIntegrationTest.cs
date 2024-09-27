using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using TechChallenge.API.Register;
using TechChallenge.API.Register.Models.Requests;
using TechChallenge.Infrastructure;
using TechChallenge.Tests.Helpers;

namespace TechChallenge.Tests.Integrations;

public class RegisterContactsIntegrationTest : IClassFixture<CustomApplicationFactory<Program>>
{
    private readonly CustomApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public RegisterContactsIntegrationTest(CustomApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task PostContact_ShouldCreateNewEntry()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<AppDbContext>();
        Utilities.ReinitializeDbForTests(db);

        var request = new RegisterContactRequest()
        {
            DDD = 21,
            Email = "new_request@test.com",
            Name = "New Request",
            Phone = "987654321"
        };

        // Act
        var response = await _client.PostAsync("/api/contacts/register", HttpContentExtensions.From(request));

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    [Fact]
    public async Task PostContact_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterContactRequest()
        {
            DDD = 200,
            Email = "<invalid_email>",
            Name = "",
            Phone = "987654321"
        };

        // Act
        var response = await _client.PostAsync("/api/contacts/register", HttpContentExtensions.From(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

}
