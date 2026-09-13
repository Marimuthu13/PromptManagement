using SmartPrompt.Application.Common.Interfaces;
using BCrypt.Net;

namespace SmartPrompt.Infrastructure.Authentication;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, hashType: HashType.SHA384);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(password, hash, hashType: HashType.SHA384);
    }
}
