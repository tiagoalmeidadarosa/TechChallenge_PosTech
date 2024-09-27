using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using TechChallenge.API;
using TechChallenge.API.Models.Responses;
using TechChallenge.Infrastructure;
using TechChallenge.Tests.Helpers;

namespace TechChallenge.Tests.Integrations;

public class GetContactsIntegrationTest : IClassFixture<CustomApplicationFactory<Program>>
{
    private readonly CustomApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public GetContactsIntegrationTest(CustomApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task GetContacts_ReturnsSuccessStatusCode()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();
            Utilities.ReinitializeDbForTests(db);
        }

        var response = await _client.GetAsync("/api/contacts");

        // Act
        response.EnsureSuccessStatusCode();
        var contacts = await response.Content.ReadAs<IEnumerable<ContactResponse>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(contacts);
        Assert.NotEmpty(contacts);
    }

    [Fact]
    public async Task GetContacts_ReturnsOnlyContactsFromSpecificDDD()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();
            Utilities.ReinitializeDbForTests(db);
        }

        var response = await _client.GetAsync("/api/contacts?DDD=11");

        // Act
        response.EnsureSuccessStatusCode();
        var contacts = await response.Content.ReadAs<IEnumerable<ContactResponse>>();

        // Assert
        Assert.NotNull(contacts);
        Assert.All(contacts, contact => Assert.Equal(11, contact.DDD));
    }

    [Fact]
    public async Task GetContactById_ReturnsSuccessfully()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();
            Utilities.ReinitializeDbForTests(db);
        }

        var response = await _client.GetAsync("/api/contacts/1");

        // Act
        response.EnsureSuccessStatusCode();
        var contact = await response.Content.ReadAs<ContactResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.NetworkAuthenticationRequired, response.StatusCode);
        Assert.NotNull(contact);
        Assert.Equal(1, contact.Id);
    }

    [Fact]
    public async Task GetContactById_ReturnsNotFound()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();
            Utilities.ReinitializeDbForTests(db);
        }

        // Act
        var response = await _client.GetAsync("/api/contacts/100");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
