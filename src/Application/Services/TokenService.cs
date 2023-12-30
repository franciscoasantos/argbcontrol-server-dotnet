using Application.Contracts;
using Application.DataContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services;

public class TokenService(IConfiguration configuration) : ITokenService
{
    public string Generate(Client client)
    {
        var handler = new JwtSecurityTokenHandler();

        var key = Encoding.ASCII.GetBytes(configuration["Jwt:SecurityKey"]!);

        var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = GetClaimsIdentity(client),
            SigningCredentials = credentials,
            Expires = DateTime.UtcNow.AddMinutes(5),
        };

        var token = handler.CreateToken(tokenDescriptor);

        return handler.WriteToken(token);
    }

    private static ClaimsIdentity GetClaimsIdentity(Client client)
    {
        var ci = new ClaimsIdentity();

        ci.AddClaim(new(ClaimTypes.Name, client.Id));

        foreach (var role in client.Roles)
            ci.AddClaim(new(ClaimTypes.Role, role));

        return ci;
    }
}
