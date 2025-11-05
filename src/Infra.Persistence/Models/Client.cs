using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ArgbControl.Api.Infrastructure.Persistence.Models;

public class Client
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ObjectId { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string[]? Roles { get; set; }
    public string? SecretHash { get; set; }
}
