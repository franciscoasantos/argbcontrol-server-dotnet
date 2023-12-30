namespace Application.Contracts;

public interface IHashService
{
    string GenerateHash(string rawData);
    bool IsValidHash(string rawData, string hashedData);
}