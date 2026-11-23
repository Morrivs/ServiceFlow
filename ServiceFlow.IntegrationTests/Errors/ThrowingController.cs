using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceFlow.Application.Common;
using ServiceFlow.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ServiceFlow.IntegrationTests.Errors
{
    [ApiController, Route("test-errors")]
    public class ThrowingController : ControllerBase
    {
        [HttpGet("domain")] public IActionResult Domain() => throw new DomainException("Regla violada");
        [HttpGet("notfound")] public IActionResult NotFoundErr() => throw new NotFoundException("Customer", 99);
        [HttpGet("conflict")] public IActionResult Conflict() => throw new ConflictException("Estado inválido");
        [HttpGet("concurrency")] public IActionResult Concurrency() => throw new DbUpdateConcurrencyException("x");
        [HttpGet("boom")] public IActionResult Boom() => throw new InvalidOperationException("secreto interno");
        [HttpGet("validation")]
        public IActionResult Validation() =>
            throw new RequestValidationException(new Dictionary<string, string[]> { ["Email"] = ["The email address is invalid."] });

        [HttpPost("model")]
        public IActionResult Model([FromBody] ModelDto dto) => Ok();
    }

    public record ModelDto([property: Required] string Name);
}
