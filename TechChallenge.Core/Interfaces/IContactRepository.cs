using TechChallenge.Core.Entities;

namespace TechChallenge.Core.Interfaces
{
    public interface IContactRepository
    {
        Task<IList<Contact>> GetAllAsync(int? ddd);

        Task<bool> Exists(int id);

        Task<Contact?> GetByIdAsync(int id);

        Task CreateAsync(Contact contact);

        Task UpdateAsync(Contact contact);

        Task DeleteAsync(int id);
    }
}
