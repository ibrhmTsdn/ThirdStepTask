namespace Common.Exceptions
{
    public abstract class BaseException : Exception
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }

        protected BaseException(string message, int statusCode, string errorCode)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }

        protected BaseException(string message, int statusCode, string errorCode, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }

    public class NotFoundException : BaseException
    {
        public NotFoundException(string message)
            : base(message, 404, "NOT_FOUND")
        {
        }

        public NotFoundException(string entity, object key)
            : base($"{entity} with key '{key}' was not found.", 404, "NOT_FOUND")
        {
        }
    }

    public class BadRequestException : BaseException
    {
        public BadRequestException(string message)
            : base(message, 400, "BAD_REQUEST")
        {
        }
    }

    public class ValidationException : BaseException
    {
        public List<string> ValidationErrors { get; }

        public ValidationException(List<string> errors)
            : base("One or more validation errors occurred.", 400, "VALIDATION_ERROR")
        {
            ValidationErrors = errors;
        }

        public ValidationException(string error)
            : base(error, 400, "VALIDATION_ERROR")
        {
            ValidationErrors = new List<string> { error };
        }
    }

    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string message = "Unauthorized access.")
            : base(message, 401, "UNAUTHORIZED")
        {
        }
    }

    public class ForbiddenException : BaseException
    {
        public ForbiddenException(string message = "Access forbidden.")
            : base(message, 403, "FORBIDDEN")
        {
        }
    }

    public class ConflictException : BaseException
    {
        public ConflictException(string message)
            : base(message, 409, "CONFLICT")
        {
        }
    }

    public class InternalServerException : BaseException
    {
        public InternalServerException(string message = "An internal server error occurred.")
            : base(message, 500, "INTERNAL_SERVER_ERROR")
        {
        }

        public InternalServerException(string message, Exception innerException)
            : base(message, 500, "INTERNAL_SERVER_ERROR", innerException)
        {
        }
    }
}
