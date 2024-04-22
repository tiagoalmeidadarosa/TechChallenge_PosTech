using TechChallenge_Fase01.Core.Entities;

namespace TechChallenge_Fase01.Core.Interfaces
{
    public interface IContactRepository
    {
        public Task<IList<Contact>> GetAllAsync(int? ddd);

        public Task<Contact?> GetByIdAsync(int id);

        public Task CreateAsync(Contact contact);

        public Task UpdateAsync(Contact contact);

        public Task DeleteAsync(Contact contact);
    }
}
