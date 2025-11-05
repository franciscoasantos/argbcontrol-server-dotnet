using System.Text.Json.Serialization;

namespace ArgbControl.Api.Contracts.Response;

public class TokenResponse
{
    [JsonPropertyName("token")]
    public required string Token { get; set; }

    [JsonPropertyName("expires_in")]
    public required int ExpiresIn { get; set; }
}
