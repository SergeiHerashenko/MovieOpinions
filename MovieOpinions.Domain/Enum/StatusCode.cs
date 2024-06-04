namespace MovieOpinions.Domain.Enum
{
    public enum StatusCode
    {
        OK = 200,
        InternalServerError = 500,
        BlockedUser = 423,
        DeleteUser = 410,
        NotFound = 404
    }
}
