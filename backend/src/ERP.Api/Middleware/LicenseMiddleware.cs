using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using ERP.Application.Services;
using Microsoft.AspNetCore.Http;

namespace ERP.Api.Middleware;

public class LicenseMiddleware
{
    private readonly RequestDelegate _next;

    public LicenseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILicenseService licenseService)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        var method = context.Request.Method;

        // CORS preflight requests (OPTIONS), swagger or public license endpoints
        if (method == HttpMethods.Options ||
            path.StartsWith("/swagger") ||
            path == "/api/auth/login" ||
            path == "/api/auth/refresh" ||
            path == "/api/license/status" ||
            path == "/api/license/activate")
        {
            await _next(context);
            return;
        }

        var (isValid, _, message) = await licenseService.CheckLicenseAsync();
        if (!isValid)
        {
            context.Response.StatusCode = (int)HttpStatusCode.PaymentRequired; // 402 Payment Required
            context.Response.ContentType = "application/json";

            var responsePayload = new
            {
                status = "LicenseRequired",
                error = message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(responsePayload));
            return;
        }

        await _next(context);
    }
}
