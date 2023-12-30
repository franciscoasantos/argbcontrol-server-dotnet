using Infra.Persistence.Models;
using Infra.Persistence.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Infra.Persistence;

public class ClientsRepository : IClientsRepository
{
    private readonly IMongoCollection<Client> _clientsCollection;

    public ClientsRepository(IOptions<MongoSettings> mongoSettinigs)
    {
        var mongoClient = new MongoClient(
            mongoSettinigs.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            mongoSettinigs.Value.DatabaseName);

        _clientsCollection = mongoDatabase.GetCollection<Client>(
            mongoSettinigs.Value.ClientsCollectionName);
    }

    public async Task<List<Client>> GetAsync() =>
        await _clientsCollection.Find(_ => true).ToListAsync();

    public async Task<Client?> GetAsync(string id) =>
        await _clientsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Client newClient) =>
        await _clientsCollection.InsertOneAsync(newClient);

    public async Task UpdateAsync(string id, Client updatedClient) =>
        await _clientsCollection.ReplaceOneAsync(x => x.Id == id, updatedClient);

    public async Task RemoveAsync(string id) =>
        await _clientsCollection.DeleteOneAsync(x => x.Id == id);
}