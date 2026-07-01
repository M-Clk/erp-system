using System;
using System.Collections.Generic;

namespace ERP.Application.Common.License;

public class LicenseInfo
{
    public string CustomerName { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
    public int MaxUsers { get; set; }
    public List<string> AllowedFeatures { get; set; } = new();
}

public class LicenseContainer
{
    public string DataJson { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}
