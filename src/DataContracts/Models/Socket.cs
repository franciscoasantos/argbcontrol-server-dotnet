using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataContracts.Models
{
    public class Socket
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [BsonElement("Id")]
        public long SocketId { get; set; }
        public long ClientId { get; set; }
        public long UserId { get; set; } 
    }
}
