using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using ServiceFlow.API.Erros;
using ServiceFlow.Infrastructure;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ServiceFlow API",
        Version = "v1",
        Description = "API REST para gestionar clientes, solicitudes de servicio, cotizaciones y órdenes de trabajo."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Escribe el token JWT con el formato: Bearer {token}.",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        var pd = ctx.ProblemDetails;
        var status = pd.Status ?? ctx.HttpContext.Response.StatusCode;
        pd.Status = status;
        pd.Type = ProblemTypes.TypeFor(status);
        pd.Title = ProblemTypes.TitleFor(status);
        pd.Extensions["traceId"] = Activity.Current?.Id ?? ctx.HttpContext.TraceIdentifier;

        if (string.IsNullOrWhiteSpace(pd.Detail))
            pd.Detail = ProblemTypes.DefaultDetailFor(status);

        if (pd is not ValidationProblemDetails && !pd.Extensions.ContainsKey("errors"))
        {
            pd.Extensions["errors"] = new Dictionary<string, string[]>
            {
                [ProblemTypes.KeyFor(status)] = [pd.Detail!]
            };
        }
    };
});

var app = builder.Build();

app.UseExceptionHandler(); 
app.UseStatusCodePages();

// Configure the HTTP request pipeline.
// RNF-008: Swagger disponible en todos los entornos, no solo en Development.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ServiceFlow API v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


public partial class Program { }