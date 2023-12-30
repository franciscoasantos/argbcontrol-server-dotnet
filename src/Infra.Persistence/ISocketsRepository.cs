using Infra.Persistence.Models;

namespace Infra.Persistence;

public interface ISocketsRepository
{
    Task CreateAsync(Socket newSocket);
    Task<List<Socket>> GetAsync();
    Task<Socket?> GetAsync(string id);
    Task<Socket?> GetByClientIdAsync(string id);
    Task RemoveAsync(string id);
    Task UpdateAsync(string id, Socket updatedSocket);
}