namespace dotnet_backend_api.Models;

public class UserModel
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public bool IsMfaEnabled { get; set; }
}