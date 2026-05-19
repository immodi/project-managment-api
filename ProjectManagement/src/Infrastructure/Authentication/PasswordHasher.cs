using System.Security.Cryptography;
using System.Text;
using Application.Interfaces;

namespace Infrastructure.Authentication;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    public bool Verify(string password, string hash)
    {
        return Hash(password) == hash;
    }
}