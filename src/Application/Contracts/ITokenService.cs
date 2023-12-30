using Application.DataContracts;

namespace Application.Contracts;

public interface ITokenService
{
    string Generate(Client client);
}
