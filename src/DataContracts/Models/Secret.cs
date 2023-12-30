using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataContracts.Models
{
    public class Secret
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [BsonElement("Id")]
        public long DeviceId { get; set; }
        public string? SecretHash { get; set; }
    }
}
