using ERP.Application.Dto;
using ERP.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/reference-data")]
[Authorize]
public class ReferenceDataController(IReferenceDataService referenceData) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ReferenceDataDto>> Get(CancellationToken cancellationToken)
        => Ok(await referenceData.GetAsync(cancellationToken));

    [HttpPost("brands")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<LookupDto>> CreateBrand(CreateLookupRequest request, CancellationToken cancellationToken)
        => Ok(await referenceData.CreateBrandAsync(request, cancellationToken));

    [HttpPost("categories")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<LookupDto>> CreateCategory(CreateLookupRequest request, CancellationToken cancellationToken)
        => Ok(await referenceData.CreateCategoryAsync(request, cancellationToken));

    [HttpPost("units")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<LookupDto>> CreateUnit(CreateLookupRequest request, CancellationToken cancellationToken)
        => Ok(await referenceData.CreateUnitAsync(request, cancellationToken));

    [HttpPost("warehouses")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<LookupDto>> CreateWarehouse(CreateLookupRequest request, CancellationToken cancellationToken)
        => Ok(await referenceData.CreateWarehouseAsync(request, cancellationToken));
}
