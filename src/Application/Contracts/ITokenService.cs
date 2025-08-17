using Application.DataContracts;

namespace Application.Contracts;

public interface ITokenService
{
    TokenInfo Generate(Client client);
}
