namespace Authorization.Application.Interfaces.Security
{
    public interface IHasher
    {
        string Hash(string value);

        bool Verify(string value, string hash);
    }
}
