namespace Authorization.Cookie
{
    public interface ICookieProvider
    {
        void SetCookies(string accessToken, string refreshToken);

        void ClearCookies();
    }
}
