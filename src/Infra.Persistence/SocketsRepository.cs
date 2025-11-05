using ArgbControl.Api.Infrastructure.Persistence.Models;
using ArgbControl.Api.Infrastructure.Persistence.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ArgbControl.Api.Infrastructure.Persistence;

public class SocketsRepository : ISocketsRepository
{
    private readonly IMongoCollection<Socket> _socketsCollection;

    public SocketsRepository(IOptions<MongoSettings> mongoSettinigs)
    {
        var mongoClient = new MongoClient(
            mongoSettinigs.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            mongoSettinigs.Value.DatabaseName);

        _socketsCollection = mongoDatabase.GetCollection<Socket>(
            mongoSettinigs.Value.SocketsCollectionName);
    }

    public async Task<List<Socket>> GetAsync() =>
        await _socketsCollection.Find(_ => true).ToListAsync();

    public async Task<Socket?> GetAsync(string id) =>
        await _socketsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<Socket?> GetByClientIdAsync(string id) =>
        await _socketsCollection.Find(x => (x.Clients ?? new string[0]).Any(w => w == id)).FirstOrDefaultAsync();

    public async Task CreateAsync(Socket newSocket) =>
        await _socketsCollection.InsertOneAsync(newSocket);

    public async Task UpdateAsync(string id, Socket updatedSocket) =>
        await _socketsCollection.ReplaceOneAsync(x => x.Id == id, updatedSocket);

    public async Task RemoveAsync(string id) =>
        await _socketsCollection.DeleteOneAsync(x => x.Id == id);
}