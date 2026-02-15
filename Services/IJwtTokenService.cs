using System.Security.Claims;

namespace ImageProcessing.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(string username, string role);
    }
}
