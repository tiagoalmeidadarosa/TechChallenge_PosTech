using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using TechChallenge_Fase01.API.Models.Requests;
using TechChallenge_Fase01.API.Models.Responses;
using TechChallenge_Fase01.Infrastructure;
using TechChallenge_Fase01.Tests.Helpers;

namespace TechChallenge_Fase01.Tests.Controllers;

public class ContactsTests : IClassFixture<CustomApplicationFactory<Program>>
{
    private readonly CustomApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ContactsTests(CustomApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
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
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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

    [Fact]
    public async Task PostContact_ShouldCreateNewEntry()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<AppDbContext>();
        Utilities.ReinitializeDbForTests(db);

        var request = new ContactRequest()
        {
            DDD = 21,
            Email = "new_request@test.com",
            Name = "New Request",
            Phone = "987654321"
        };

        // Act
        var response = await _client.PostAsync("/api/contacts", HttpContentExtensions.From(request));

        response.EnsureSuccessStatusCode();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(4, db.Contacts.Count());
    }

    [Fact]
    public async Task PostContact_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new ContactRequest()
        {
            DDD = 200,
            Email = "<invalid_email>",
            Name = "",
            Phone = "987654321"
        };

        // Act
        var response = await _client.PostAsync("/api/contacts", HttpContentExtensions.From(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutContact_ShouldUpdateData()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<AppDbContext>();
        Utilities.ReinitializeDbForTests(db);

        var request = new ContactRequest()
        {
            DDD = 21,
            Email = "update_request@test.com",
            Name = "Update Request",
            Phone = "987654321"
        };

        // Act
        var responsePut = await _client.PutAsync("/api/contacts/3", HttpContentExtensions.From(request));
        responsePut.EnsureSuccessStatusCode();

        var responseGet = await _client.GetAsync("/api/contacts/3");
        responseGet.EnsureSuccessStatusCode();

        var contact = await responseGet.Content.ReadAs<ContactResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, responsePut.StatusCode);
        Assert.NotNull(contact);

        Assert.Equal("Update Request", contact.Name);
        Assert.Equal(21, contact.DDD);
        Assert.Equal("update_request@test.com", contact.Email);
        Assert.Equal("987654321", contact.Phone);
    }

    [Fact]
    public async Task PutContact_ShouldReturnNotFound()
    {
        // Arrange
        var request = new ContactRequest()
        {
            DDD = 21,
            Email = "update_request@test.com",
            Name = "Update Request",
            Phone = "987654321"
        };

        // Act
        var responsePut = await _client.PutAsync("/api/contacts/312", HttpContentExtensions.From(request));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, responsePut.StatusCode);
    }

    [Fact]
    public async Task PutContact_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new ContactRequest()
        {
            DDD = 200,
            Email = "<invalid_email>",
            Name = "",
            Phone = "987654321"
        };

        // Act
        var responsePut = await _client.PutAsync("/api/contacts/312", HttpContentExtensions.From(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, responsePut.StatusCode);
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
        var responseDelete = await _client.DeleteAsync("/api/contacts/2");
        responseDelete.EnsureSuccessStatusCode();

        var responseGet = await _client.GetAsync("/api/contacts");
        responseGet.EnsureSuccessStatusCode();

        var contacts = await responseGet.Content.ReadAs<IEnumerable<ContactResponse>>();

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, responseDelete.StatusCode);
        Assert.NotNull(contacts);

        Assert.Equal(2, contacts.Count());
        Assert.DoesNotContain(contacts, contact => contact.Id == 2);
    }

    [Fact]
    public async Task DeleteContact_ShouldReturnNotFound()
    {
        // Arrange
        // Act
        var response = await _client.DeleteAsync("/api/contacts/31");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
