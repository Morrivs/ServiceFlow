using Microsoft.AspNetCore.Mvc;
using ServiceFlow.API.Erros;
using ServiceFlow.Infrastructure;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


public partial class Program { }