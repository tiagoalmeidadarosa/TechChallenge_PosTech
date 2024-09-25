using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using TechChallenge.API.Update;
using TechChallenge.API.Update.Models.Requests;
using TechChallenge.Infrastructure;
using TechChallenge.Tests.Helpers;

namespace TechChallenge.Tests.Integrations;

public class UpdateContactsIntegrationTest : IClassFixture<CustomApplicationFactory<Program>>
{
    private readonly CustomApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public UpdateContactsIntegrationTest(CustomApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task PutContact_ShouldUpdateData()
    {
        // Arrange
        using IServiceScope scope = _factory.Services.CreateScope();

        IServiceProvider scopedServices = scope.ServiceProvider;
        AppDbContext db = scopedServices.GetRequiredService<AppDbContext>();
        Utilities.ReinitializeDbForTests(db);

        UpdateContactRequest request = new UpdateContactRequest()
        {
            DDD = 21,
            Email = "update_request@test.com",
            Name = "Update Request",
            Phone = "987654321"
        };

        // Act
        HttpResponseMessage responsePut = await _client.PutAsync("/api/contacts/update/3", HttpContentExtensions.From(request));

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, responsePut.StatusCode);
    }

    [Fact]
    public async Task PutContact_ShouldReturnNotFound()
    {
        // Arrange
        using IServiceScope scope = _factory.Services.CreateScope();

        IServiceProvider scopedServices = scope.ServiceProvider;
        AppDbContext db = scopedServices.GetRequiredService<AppDbContext>();
        Utilities.ReinitializeDbForTests(db);

        UpdateContactRequest request = new UpdateContactRequest()
        {
            DDD = 21,
            Email = "update_request@test.com",
            Name = "Update Request",
            Phone = "987654321"
        };

        // Act
        HttpResponseMessage responsePut = await _client.PutAsync("/api/contacts/update/312", HttpContentExtensions.From(request));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, responsePut.StatusCode);
    }

    [Fact]
    public async Task PutContact_ShouldReturnBadRequest()
    {
        // Arrange
        UpdateContactRequest request = new UpdateContactRequest()
        {
            DDD = 200,
            Email = "<invalid_email>",
            Name = "",
            Phone = "987654321"
        };

        // Act
        HttpResponseMessage responsePut = await _client.PutAsync("/api/contacts/update/312", HttpContentExtensions.From(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, responsePut.StatusCode);
    }
}
