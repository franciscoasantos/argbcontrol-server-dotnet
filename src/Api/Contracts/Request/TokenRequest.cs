﻿using System.Text.Json.Serialization;

namespace Api.Contracts.Request;

public class TokenRequest
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("secret")]
    public required string Secret { get; set; }
}
