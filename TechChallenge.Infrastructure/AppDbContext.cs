using Microsoft.EntityFrameworkCore;
using TechChallenge.Core.Entities;

namespace TechChallenge.Infrastructure
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Contact> Contacts { get; set; }
    }
}
