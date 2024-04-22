using Microsoft.EntityFrameworkCore;
using TechChallenge_Fase01.Core.Entities;

namespace TechChallenge_Fase01.Infrastructure
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Contact> Contacts { get; set; }
    }
}
