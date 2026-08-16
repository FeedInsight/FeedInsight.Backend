using FeedInsight.Application.Features.CompanyCustomers.Commands.DeleteCompanyCustomer;
using FeedInsight.Application.Features.Customers.Commands.CreateCompanyCustomer;
using FeedInsight.Application.Features.Customers.Commands.LockCompanyCustomer;
using FeedInsight.Application.Features.Customers.Commands.UnlockCompanyCustomer;
using FeedInsight.Application.Features.Customers.Commands.UpdateCompanyCustomer;
using FeedInsight.Application.Features.Customers.Queries.GetCompanyCustomerById;
using FeedInsight.Application.Features.Customers.Queries.GetCompanyCustomers;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedInsight.API.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = Role.ProductOwner, Policy = "DevelopmentAppOnly")]
public class CustomersController : ApiController
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("company-customers")]
    public async Task<IActionResult> CreateCompanyCustomer(
        [FromBody] CreateCompanyCustomerCommand command)
    {
        var result = await _mediator.SendAsync(command);

        return result.Match(
            userId => OkResponse(new
            {
                UserId = userId,
                Message = "Customer account created successfully."
            }),
            errors => Problem(errors));
    }

    [HttpGet("company-customers")]
    public async Task<IActionResult> GetCompanyCustomers(
        [FromQuery] GetCompanyCustomersQuery query)
    {
        var result = await _mediator.SendAsync(query);

        return result.Match(
            paginatedResult => OkPaginated(
                data: paginatedResult.Items,
                currentPage: query.Page,
                pageSize: query.PageSize,
                totalItems: paginatedResult.TotalCount),
            errors => Problem(errors));
    }

    [HttpGet("company-customers/{id:guid}")]
    public async Task<IActionResult> GetCompanyCustomerById(Guid id)
    {
        var query = new GetCompanyCustomerByIdQuery(id);

        var result = await _mediator.SendAsync(query);

        return result.Match(
            customer => OkResponse(customer),
            errors => Problem(errors));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCustomer(
        Guid id,
        [FromBody] UpdateCustomerRequest request)
    {
        var command = new UpdateCustomerCommand(
            id,
            request.FirstName,
            request.LastName,
            request.Email);

        var result = await _mediator.SendAsync(command);

        return result.Match(
            _ => OkResponse(new
            {
                Message = "Customer updated successfully."
            }),
            errors => Problem(errors));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        var command = new DeleteCompanyCustomerCommand(id);

        var result = await _mediator.SendAsync(command);

        return result.Match(
            _ => OkResponse(new
            {
                Message = "Customer deleted successfully."
            }),
            errors => Problem(errors));
    }

    [HttpPost("company-customers/{id:guid}/lock")]
    public async Task<IActionResult> LockCompanyCustomer(
        Guid id,
        [FromBody] LockCompanyCustomerRequest request)
    {
        var command = new LockCompanyCustomerCommand(
            id,
            request.Reason);

        var result = await _mediator.SendAsync(command);

        return result.Match(
            _ => OkResponse(new
            {
                Message = "Customer locked successfully."
            }),
            errors => Problem(errors));
    }

    [HttpPost("company-customers/{id:guid}/unlock")]
    public async Task<IActionResult> UnlockCompanyCustomer(Guid id)
    {
        var command = new UnlockCompanyCustomerCommand(id);

        var result = await _mediator.SendAsync(command);

        return result.Match(
            _ => OkResponse(new
            {
                Message = "Customer unlocked successfully."
            }),
            errors => Problem(errors));
    }

    public record UpdateCustomerRequest(
        string FirstName,
        string LastName,
        string Email);

    public record LockCompanyCustomerRequest(
        string? Reason);
}