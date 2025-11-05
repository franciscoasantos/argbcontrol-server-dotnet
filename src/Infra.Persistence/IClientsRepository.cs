using ArgbControl.Api.Infrastructure.Persistence.Models;

namespace ArgbControl.Api.Infrastructure.Persistence;

public interface IClientsRepository
{
    Task CreateAsync(Client newClient);
    Task<List<Client>> GetAsync();
    Task<Client?> GetAsync(string id);
    Task RemoveAsync(string id);
    Task UpdateAsync(string id, Client updatedClient);
}