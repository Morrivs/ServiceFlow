using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceFlow.Application.Common;
using ServiceFlow.Domain.Common;

namespace ServiceFlow.API.Erros
{
    internal sealed class GlobalExceptionHandler(
        IProblemDetailsService problemDetails,
        ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
        {
            (int Status, string Detail, IDictionary<string, string[]>? Errors) r = ex switch
            {
                RequestValidationException v => (400, v.Message, v.Errors),
                BadHttpRequestException => (400, "The request is malformed.", null),
                NotFoundException n => (404, n.Message, null),
                ConflictException c => (409, c.Message, null),
                DbUpdateConcurrencyException => (409, "The resource was modified by someone else. Reload and try again.", null),
                DomainException d => (422, d.Message, null),
                _ => (500, "An unexpected error occurred.", null)
            };

            if (r.Status >= 500)
                logger.LogError(ex, "Unhandled exception on {Path}", ctx.Request.Path);
            else
                logger.LogWarning("{Type}: {Message}", ex.GetType().Name, ex.Message);

            var problem = new ProblemDetails { Status = r.Status, Detail = r.Detail };
            if (r.Errors is not null) problem.Extensions["errors"] = r.Errors;

            ctx.Response.StatusCode = r.Status;
            return await problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = ctx,
                ProblemDetails = problem,
                Exception = ex
            });
        }
    }
}
