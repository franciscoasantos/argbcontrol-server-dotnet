using Infra.Persistence.Models;

namespace Infra.Persistence;

public interface IClientsRepository
{
    Task CreateAsync(Client newClient);
    Task<List<Client>> GetAsync();
    Task<Client?> GetAsync(string id);
    Task RemoveAsync(string id);
    Task UpdateAsync(string id, Client updatedClient);
}