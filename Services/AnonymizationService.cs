using System.Security.Cryptography;

namespace BurnoutMonitoring.Services;

public interface IAnonymizationService
{
    string GenerateSessionHash();
}

public class AnonymizationService : IAnonymizationService
{
    public string GenerateSessionHash()
    {
        // 32 случайных байта → 64 символа hex (SHA-256-подобная длина)
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}