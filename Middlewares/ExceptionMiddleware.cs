using System.Text.Json;
using TodoApi.Dto;
using TodoApi.Exceptions;

namespace TodoApi.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (DomainException dex)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            
            var result = JsonSerializer.Serialize(new PadraoErroResponse(dex.Message));
            
            await httpContext.Response.WriteAsync(result);
        }
        catch (Exception)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            
            var result = JsonSerializer.Serialize(new PadraoErroResponse("Ocorreu um erro interno. Tente novamente mais tarde."));
            
            await httpContext.Response.WriteAsync(result);
        }
    }
}