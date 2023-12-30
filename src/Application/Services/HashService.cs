using System.Security.Cryptography;
using System.Text;
using Application.Contracts;

namespace Application.Services;

public class HashService : IHashService
{
    private static byte[] GenerateSalt()
    {
        byte[] salt = new byte[32];

        using var rng = RandomNumberGenerator.Create();

        rng.GetBytes(salt);

        return salt;
    }

    public string GenerateHash(string rawData)
    {
        byte[] salt = GenerateSalt();
        byte[] dataBytes = Encoding.UTF8.GetBytes(rawData);
        byte[] saltedData = new byte[dataBytes.Length + salt.Length];
        Array.Copy(dataBytes, 0, saltedData, 0, dataBytes.Length);
        Array.Copy(salt, 0, saltedData, dataBytes.Length, salt.Length);
        byte[] hash = SHA256.HashData(saltedData);

        return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
    }

    public bool IsValidHash(string rawData, string hashedData)
    {
        string[] parts = hashedData.Split(':');
        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] storedHash = Convert.FromBase64String(parts[1]);
        byte[] dataBytes = Encoding.UTF8.GetBytes(rawData);
        byte[] saltedData = new byte[dataBytes.Length + salt.Length];
        Array.Copy(dataBytes, 0, saltedData, 0, dataBytes.Length);
        Array.Copy(salt, 0, saltedData, dataBytes.Length, salt.Length);
        byte[] hash = SHA256.HashData(saltedData);

        return storedHash.SequenceEqual(hash);
    }
}