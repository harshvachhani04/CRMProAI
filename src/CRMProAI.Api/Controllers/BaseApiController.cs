using Asp.Versioning;
using CRMProAI.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace CRMProAI.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected string CorrelationId =>
        HttpContext.Items["CorrelationId"]?.ToString() ?? HttpContext.TraceIdentifier;

    protected ActionResult<ApiResponse<T>> OkResponse<T>(T data, string message = "Success")
    {
        return Ok(new ApiResponse<T>(data, message));
    }

    protected ActionResult<ApiResponse<T>> CreatedResponse<T>(
        string actionName, 
        object routeValues, 
        T data, 
        string message = "Resource created successfully")
    {
        return CreatedAtAction(actionName, routeValues, new ApiResponse<T>(data, message));
    }
}
