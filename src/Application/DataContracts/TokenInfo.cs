﻿using System.Text.Json.Serialization;

namespace Application.DataContracts;

public class TokenInfo
{
    [JsonPropertyName("token")]
    public string Token { get; set; }
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
