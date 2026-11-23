using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceFlow.Application.Common
{
    public class NotFoundException(string entity, object key)
        : Exception($"{entity} with id '{key}' was not found.");

    public class ConflictException(string message) : Exception(message);

    public class RequestValidationException(IDictionary<string, string[]> errors)
        : Exception("One or more validation errors occurred.")
    {
        public IDictionary<string, string[]> Errors { get; } = errors;
    }
}
