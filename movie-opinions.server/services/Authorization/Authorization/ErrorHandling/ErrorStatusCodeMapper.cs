using Authorization.ErrorHandling.CodesStatus;

namespace Authorization.ErrorHandling
{
    public class ErrorStatusCodeMapper : IErrorStatusCodeMapper
    {
        public int GetStatusCode(string errorCode)
        {
            if (CodesCatalog.Codes.TryGetValue(errorCode, out var statusCode))
                return statusCode;

            return StatusCodes.Status500InternalServerError;
        }
    }
}
