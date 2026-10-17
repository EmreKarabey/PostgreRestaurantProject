namespace CofeeAdmin.Services.Jwt
{
    public interface IJwtService
    {
        string GenerateToken(string username, string role = "Admin");
    }
}
