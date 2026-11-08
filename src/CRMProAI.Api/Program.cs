using CRMProAI.Api.Extensions;
using CRMProAI.Api.Middleware;
using CRMProAI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Register Infrastructure (EF Core PostgreSQL DbContext, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Register shared API conventions (Controllers, API Versioning, OpenAPI, FluentValidation)
builder.Services.AddSharedApiConventions(builder.Configuration);

var app = builder.Build();

// 1. Correlation ID Middleware (First in pipeline to track all downstream calls)
app.UseMiddleware<CorrelationIdMiddleware>();

// 2. Global Exception Handling Middleware (Translates unhandled exceptions to RFC 7807 ProblemDetails)
app.UseMiddleware<GlobalExceptionMiddleware>();

// 3. OpenAPI Documentation
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 4. Map API Controllers
app.MapControllers();

app.Run();

// Assembly entrypoint marker for integration tests
public partial class Program { }
