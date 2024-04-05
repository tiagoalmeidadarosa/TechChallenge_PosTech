using Microsoft.EntityFrameworkCore;
using TechChallenge_Fase01.Models;

namespace TechChallenge_Fase01.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Contact> Contacts { get; set; }
    }
}
