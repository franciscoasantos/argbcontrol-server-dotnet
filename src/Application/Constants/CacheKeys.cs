namespace ArgbControl.Api.Application.Constants;

public static class CacheKeys
{
    private const string Prefix = "ArgbControl:";
    
    public static string GetAuthenticationKey(string clientId) => $"{Prefix}Auth:{clientId}";
}
