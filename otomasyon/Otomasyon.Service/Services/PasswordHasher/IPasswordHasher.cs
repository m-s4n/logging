namespace Otomasyon.Service.Services.PasswordHasher;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hashedPassword);
}