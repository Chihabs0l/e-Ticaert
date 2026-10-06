using System.Security.Cryptography;
using System.Text;

namespace ShoppingApp.Helpers;

/// <summary>
/// DEMO ONLY — SHA-256 is NOT sufficient for production.
/// Use ASP.NET Core Identity or BCrypt in a real application.
/// </summary>
public static class PasswordHelper
{
    public static string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes("shoppingapp_salt_" + password));
        return Convert.ToHexString(bytes);
    }

    public static bool Verify(string password, string hash)
        => Hash(password).Equals(hash, StringComparison.OrdinalIgnoreCase);
}
