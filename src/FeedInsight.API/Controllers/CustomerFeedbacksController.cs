using FeedInsight.Application.Features.CustomerFeedbacks.Commands.Development.AddCompanyFeedbackComment;
using FeedInsight.Application.Features.CustomerFeedbacks.Development.Commands.SubmitCompanyCustomerFeedback;
using FeedInsight.Application.Features.CustomerFeedbacks.Development.Queries.GetCompanyCustomerFeedbacks;
using FeedInsight.Application.Features.CustomerFeedbacks.Development.Queries.GetProductOwnerComments;
using FeedInsight.Application.Features.CustomerFeedbacks.Queries.GetCustomerFeedbacks;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedInsight.API.Controllers;

[Route("api/feedbacks")]
public class CustomerFeedbacksController : ApiController
{
    private readonly IMediator _mediator;

    public CustomerFeedbacksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Production - Product Owner
    // GET: /api/feedbacks
    [HttpGet]
    [Authorize(Roles = Role.ProductOwner)]
    public async Task<IActionResult> GetProductOwnerFeedbacks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetCustomerFeedbacksQuery(page, pageSize);

        var result = await _mediator.SendAsync(query);

        return result.Match(
            paginatedResult => OkPaginated(
                data: paginatedResult.Items,
                currentPage: query.Page,
                pageSize: query.PageSize,
                totalItems: paginatedResult.TotalCount
            ),
            errors => Problem(errors)
        );
    }

    // Development - Company Customer
    // POST: /api/feedbacks/development/customer
    [HttpPost("development/customer")]
    [Authorize(Roles = Role.CompanyCustomer)]
    public async Task<IActionResult> SubmitCustomerFeedback(
        [FromBody] SubmitCompanyCustomerFeedbackCommand command)
    {
        var result = await _mediator.SendAsync(command);

        return result.Match(
            id => OkResponse(new
            {
                FeedbackId = id,
                Message = "Feedback submitted successfully."
            }),
            errors => Problem(errors)
        );
    }

    // Development - Product Owner
    // GET: /api/feedbacks/development/company
    [HttpGet("development/company")]
    [Authorize(Roles = Role.ProductOwner,Policy = "DevelopmentAppOnly")]
    public async Task<IActionResult> GetCompanyFeedbacks(
        [FromQuery] GetCompanyCustomerFeedbacksQuery query)
    {
        var result = await _mediator.SendAsync(query);

        return result.Match(
            paginatedResult => OkPaginated(
                data: paginatedResult.Items,
                currentPage: query.Page,
                pageSize: query.PageSize,
                totalItems: paginatedResult.TotalCount
            ),
            errors => Problem(errors)
        );
    }

    // Development - Product Owner
    // POST: /api/feedbacks/development/company/{feedbackId}/comments
    [HttpPost("development/company/{feedbackId}/comments")]
    [Authorize(
        Roles = Role.ProductOwner,
        Policy = "DevelopmentAppOnly")]
    public async Task<IActionResult> AddFeedbackComment(
        Guid feedbackId,
        [FromBody] AddCompanyFeedbackCommentRequest request)
    {
        var command = new AddCompanyFeedbackCommentCommand(
            feedbackId,
            request.Content);

        var result = await _mediator.SendAsync(command);

        return result.Match(
            id => OkResponse(new
            {
                CommentId = id,
                Message = "Comment added successfully."
            }),
            errors => Problem(errors)
        );
    }

    // Development - Company Customer
    // GET: /api/feedbacks/development/customer
    [HttpGet("development/customer")]
    [Authorize(Roles = Role.CompanyCustomer)]
    public async Task<IActionResult> GetCustomerFeedbacksWithComments(
        [FromQuery] GetProductOwnerCommentsQuery query)
    {
        var result = await _mediator.SendAsync(query);

        return result.Match(
            paginatedResult => OkPaginated(
                data: paginatedResult.Items,
                currentPage: query.Page,
                pageSize: query.PageSize,
                totalItems: paginatedResult.TotalCount
            ),
            errors => Problem(errors)
        );
    }

    public record AddCompanyFeedbackCommentRequest(string Content);
}