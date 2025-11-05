using ArgbControl.Api.Application.DataContracts;

namespace ArgbControl.Api.Application.Contracts;

public interface ITokenService
{
    TokenInfo Generate(Client client);
}
