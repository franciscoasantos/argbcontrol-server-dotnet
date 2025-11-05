namespace ArgbControl.Api.Infrastructure.Persistence.Settings;

public class MongoSettings
{
    public string? ConnectionString { get; set; }
    public string? DatabaseName { get; set; }
    public string? ClientsCollectionName { get; set; }
    public string? SocketsCollectionName { get; set; }
}
