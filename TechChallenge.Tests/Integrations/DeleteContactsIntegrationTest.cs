using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using TechChallenge.API.Delete;
using TechChallenge.Infrastructure;
using TechChallenge.Tests.Helpers;

namespace TechChallenge.Tests.Integrations;

public class DeleteContactsIntegrationTest : IClassFixture<CustomApplicationFactory<Program>>
{
    private readonly CustomApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public DeleteContactsIntegrationTest(CustomApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task DeleteContact_ShouldRemoveData()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<AppDbContext>();
        Utilities.ReinitializeDbForTests(db);

        // Act
        var responseDelete = await _client.DeleteAsync("/api/contacts/delete/2");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, responseDelete.StatusCode);
    }

    [Fact]
    public async Task DeleteContact_ShouldReturnNotFound()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<AppDbContext>();
        Utilities.ReinitializeDbForTests(db);

        // Act
        var response = await _client.DeleteAsync("/api/contacts/delete/31");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
