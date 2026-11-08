using Asp.Versioning;
using FluentValidation;

namespace CRMProAI.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedApiConventions(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Controllers & JSON Options
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            });

        // 2. API Versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // 3. Official .NET 10 OpenAPI
        services.AddOpenApi("v1");

        // 4. FluentValidation
        services.AddValidatorsFromAssembly(typeof(CRMProAI.Application.Common.ITenantContext).Assembly);

        return services;
    }
}
