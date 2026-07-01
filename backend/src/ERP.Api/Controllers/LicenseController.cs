using System.Threading.Tasks;
using ERP.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/license")]
public class LicenseController(ILicenseService licenseService) : ControllerBase
{
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var (isValid, info, message) = await licenseService.CheckLicenseAsync();
        return Ok(new
        {
            isValid,
            customerName = info?.CustomerName,
            expirationDate = info?.ExpirationDate,
            maxUsers = info?.MaxUsers,
            allowedFeatures = info?.AllowedFeatures,
            message
        });
    }

    [HttpPost("activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate([FromBody] ActivateLicenseRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.LicenseKey))
        {
            return BadRequest(new { error = "Lisans anahtarı boş olamaz." });
        }

        var success = await licenseService.ActivateLicenseAsync(request.LicenseKey);
        if (success)
        {
            return Ok(new { message = "Lisans başarıyla etkinleştirildi." });
        }

        return BadRequest(new { error = "Lisans anahtarı geçersiz veya doğrulanamadı." });
    }
}

public class ActivateLicenseRequest
{
    public string LicenseKey { get; set; } = string.Empty;
}
