using Microsoft.EntityFrameworkCore;
using TechChallenge.Core.Entities;
using TechChallenge.Core.Interfaces;
using TechChallenge.Infrastructure;

namespace TechChallenge.Infrastructure.Repository
{
    public class ContactRepository(AppDbContext dbContext) : IContactRepository
    {
        private readonly AppDbContext _dbContext = dbContext;

        public async Task CreateAsync(Contact contact)
        {
            _dbContext.Contacts.Add(contact);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Contact contact)
        {
            _dbContext.Contacts.Remove(contact);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IList<Contact>> GetAllAsync(int? ddd)
        {
            return await _dbContext.Contacts
                .Where(x => ddd == null || x.DDD == ddd)
                .ToListAsync();
        }

        public async Task<Contact?> GetByIdAsync(int id)
        {
            return await _dbContext.Contacts.FindAsync(id);
        }

        public async Task UpdateAsync(Contact contact)
        {
            _dbContext.Contacts.Update(contact);
            await _dbContext.SaveChangesAsync();
        }
    }
}
