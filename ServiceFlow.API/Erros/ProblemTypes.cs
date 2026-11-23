namespace ServiceFlow.API.Erros
{
    internal class ProblemTypes
    {
        private const string Base = "https://api.serviceflow.com/errors/";

        public static string TypeFor(int status) => Base + status switch
        {
            400 => "validation",
            401 => "unauthorized",
            403 => "forbidden",
            404 => "not-found",
            409 => "conflict",
            422 => "business-rule",
            _ => "internal-error"
        };

        public static string TitleFor(int status) => status switch
        {
            400 => "Validation Error",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            409 => "Conflict",
            422 => "Business Rule Violation",
            _ => "Internal Server Error"
        };

        public static string KeyFor(int status) => status switch
        {
            400 => "Request",
            401 => "Authentication",
            403 => "Authorization",
            404 => "NotFound",
            409 => "Conflict",
            422 => "BusinessRule",
            _ => "Server"
        };

        public static string DefaultDetailFor(int status) => status switch
        {
            400 => "The request is invalid.",
            401 => "Authentication is required to access this resource.",
            403 => "You do not have permission to perform this action.",
            404 => "The requested resource was not found.",
            409 => "The request conflicts with the current state of the resource.",
            422 => "The request violates a business rule.",
            _ => "An unexpected error occurred."
        };
    }
}
