using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using TechChallenge.Core.Entities;
using TechChallenge.Infrastructure;

namespace TechChallenge.Tests.Helpers;

public static class Utilities
{
    public static void InitializeDbForTests(AppDbContext db)
    {
        db.Contacts.AddRange(GetSeedingMessages());
        db.SaveChanges();
    }

    public static void ReinitializeDbForTests(AppDbContext db)
    {
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        db.Contacts.RemoveRange(db.Contacts);
        InitializeDbForTests(db);
    }

    public static List<Contact> GetSeedingMessages()
    {
        return new List<Contact>()
        {
            new() { Id = 1, DDD = 11, Phone = "999999999", Name = "Test 1", Email = "test_01@test.com" },
            new() { Id = 2, DDD = 11, Phone = "123456789", Name = "Test 2", Email = "test_02@test.com" },
            new() { Id = 3, DDD = 51, Phone = "888888888", Name = "Test 3", Email = "test_03@test.com" },
        };
    }
}
