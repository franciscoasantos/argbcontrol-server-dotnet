using ArgbControl.Api.Infrastructure.Persistence.Models;
using ArgbControl.Api.Infrastructure.Persistence.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ArgbControl.Api.Infrastructure.Persistence;

public class ClientsRepository : IClientsRepository
{
    private readonly IMongoCollection<Client> clientsCollection;

    public ClientsRepository(IOptions<MongoSettings> mongoSettinigs)
    {
        var mongoClient = new MongoClient(
            mongoSettinigs.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            mongoSettinigs.Value.DatabaseName);

        clientsCollection = mongoDatabase.GetCollection<Client>(
            mongoSettinigs.Value.ClientsCollectionName);
    }

    public async Task<List<Client>> GetAsync() =>
        await clientsCollection.Find(_ => true).ToListAsync();

    public async Task<Client?> GetAsync(string id) =>
        await clientsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Client newClient) =>
        await clientsCollection.InsertOneAsync(newClient);

    public async Task UpdateAsync(string id, Client updatedClient) =>
        await clientsCollection.ReplaceOneAsync(x => x.Id == id, updatedClient);

    public async Task RemoveAsync(string id) =>
        await clientsCollection.DeleteOneAsync(x => x.Id == id);
}