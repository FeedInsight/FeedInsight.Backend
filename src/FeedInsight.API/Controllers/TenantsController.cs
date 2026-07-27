using FeedInsight.Application.Features.Tenants.Commands.CreateTenantOwner;
using FeedInsight.Application.Features.Tenants.Commands.ToggleTenantStatus;
using FeedInsight.Application.Features.Tenants.Commands.UpdateTenant;
using FeedInsight.Application.Features.Tenants.Queries.GetTenants;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Tenants.Enums;
using FeedInsight.Domain.Users;
using FeedInsight.Infrastructure.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedInsight.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class TenantsController : ApiController
{
    private readonly IMediator _mediator;

    public TenantsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("my-company")]
    [Authorize(Roles = Role.ProductOwner)]
    public async Task<IActionResult> UpdateMyCompany([FromBody] UpdateTenantCommand command)
    {
        var result = await _mediator.SendAsync(command);

        return result.Match(
            success => OkResponse(new { Message = "Company details updated successfully." }),
            errors => Problem(errors)
        );
    }

    [HttpPost("my-company/owners")]
    [Authorize(Roles = Role.ProductOwner)]
    public async Task<IActionResult> CreateCompanyOwner([FromBody] CreateTenantOwnerCommand command)
    {
        var result = await _mediator.SendAsync(command);

        return result.Match(
            userId => OkResponse(new { UserId = userId, Message = "New owner added to the company successfully." }),
            errors => Problem(errors)
        );
    }

    [HttpGet("lookup")]
    [Authorize(Roles = Role.SuperAdmin)]
    public async Task<IActionResult> GetTenants(
    [FromQuery] GetTenantsQuery query)
    {
        var result = await _mediator.SendAsync(query);

        return result.Match(
            paginatedResult => OkPaginated(
                data: paginatedResult.Items,
                currentPage: query.Page ?? 1,
                pageSize: query.PageSize ?? 1,
                totalItems: paginatedResult.TotalCount
            ),
            errors => Problem(errors)
        );
    }

    [HttpPatch("{tenantId:guid}/status")]
    [Authorize(Roles = Role.SuperAdmin)]
    public async Task<IActionResult> ToggleTenantStatus(
     [FromRoute] Guid tenantId,
     [FromBody] ToggleTenantStatusRequest request)
    {
        var command = new ToggleTenantStatusCommand(
            TenantId: tenantId,
            Status: request.Status,
            Reason: request.Reason);

        var result = await _mediator.SendAsync(command);

        return result.Match(
            _ => OkResponse(new
            {
                Message = "Tenant status updated successfully."
            }),
            errors => Problem(errors)
        );
    }
}