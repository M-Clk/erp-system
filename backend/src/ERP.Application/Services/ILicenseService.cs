using System.Threading.Tasks;
using ERP.Application.Common.License;

namespace ERP.Application.Services;

public interface ILicenseService
{
    Task<(bool IsValid, LicenseInfo? Info, string Message)> CheckLicenseAsync();
    Task<bool> ActivateLicenseAsync(string licenseKey);
}
