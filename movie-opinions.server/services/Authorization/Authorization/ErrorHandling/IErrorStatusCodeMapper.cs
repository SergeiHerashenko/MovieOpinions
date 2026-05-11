namespace Authorization.ErrorHandling
{
    public interface IErrorStatusCodeMapper
    {
        int GetStatusCode(string errorCode);
    }
}
