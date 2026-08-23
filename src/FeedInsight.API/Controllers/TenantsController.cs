using FeedInsight.Application.Features.Tenants.Commands.ConfigureJira;
using FeedInsight.Application.Features.Tenants.Commands.CreateApiKey;
using FeedInsight.Application.Features.Tenants.Commands.CreateTenantOwner;
using FeedInsight.Application.Features.Tenants.Commands.RevokeApiKey;
using FeedInsight.Application.Features.Tenants.Commands.ToggleTenantStatus;
using FeedInsight.Application.Features.Tenants.Commands.UpdateTenant;
using FeedInsight.Application.Features.Tenants.Queries.GetApiKeys;
using FeedInsight.Application.Features.Tenants.Queries.GetJiraIntegration;
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

    [HttpPut("my-company/jira-config")]
    [Authorize(Roles = Role.ProductOwner)]
    public async Task<IActionResult> ConfigureJira([FromBody] ConfigureJiraCommand command)
    {
        var result = await _mediator.SendAsync(command);

        return result.Match(
            success => OkResponse(new { Message = "Jira configuration updated successfully." }),
            errors => Problem(errors)
        );
    }

    [HttpGet("my-company/api-keys")]
    [Authorize(Roles = Role.ProductOwner)]
    public async Task<IActionResult> GetApiKeys()
    {
        var result = await _mediator.SendAsync(new GetApiKeysQuery());

        return result.Match(
            keys => OkResponse(keys),
            errors => Problem(errors)
        );
    }

    [HttpPost("my-company/api-keys")]
    [Authorize(Roles = Role.ProductOwner)]
    public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyCommand command)
    {
        var result = await _mediator.SendAsync(command);

        return result.Match(
            plainTextKey => OkResponse(new { ApiKey = plainTextKey, Message = "API Key generated successfully. Please copy it now as it will not be shown again." }),
            errors => Problem(errors)
        );
    }

    [HttpDelete("my-company/api-keys/{id:guid}")]
    [Authorize(Roles = Role.ProductOwner)]
    public async Task<IActionResult> RevokeApiKey(Guid id)
    {
        var result = await _mediator.SendAsync(new RevokeApiKeyCommand(id));

        return result.Match(
            success => OkResponse(new { Message = "API Key successfully revoked." }),
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

    [HttpGet("jira")]
    public async Task<IActionResult> GetJiraIntegration()
    {
        var result = await _mediator.SendAsync(
            new GetJiraIntegrationQuery());

        return result.Match(
            integration => OkResponse(integration),
            errors => Problem(errors));
    }
}