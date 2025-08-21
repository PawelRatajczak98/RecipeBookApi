using System.Net;

namespace Application.Exceptions
{
    public class ApiException : Exception
    {
        public int StatusCode { get; set; } 
        public string? Details { get; set; } 
        public ApiException(int statusCode, string message, string? details = null) : base(message)
        {
            StatusCode = statusCode;
            Details = details;
        }
    }

    public class ValidationException: ApiException
    {
        public ValidationException(string message, string? details = "") : base((int)HttpStatusCode.BadRequest, message, details)
        {
        }
    }

    public class NotFoundException : ApiException
    {
        public NotFoundException(string message, string? details = null) : base((int)HttpStatusCode.NotFound, message, details)
        {
        }
    }

    public class UnauthorizedException : ApiException
    {
        public UnauthorizedException(string message, string? details = null) : base((int)HttpStatusCode.Unauthorized, message, details)
        {
        }
    }

    public class ForbiddenException : ApiException
    {
        public ForbiddenException(string message, string? details = null) : base((int)HttpStatusCode.Forbidden, message, details)
        {
        }
    }

    public class OperationCanceledException : ApiException
    {
        public OperationCanceledException(string message, string? details = null) : base((int)HttpStatusCode.RequestTimeout, message, details)
        {

        }
    }
}