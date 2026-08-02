using FeedInsight.Application.Features.Categories.Commands.CreateCategory;
using FeedInsight.Application.Features.Categories.Commands.DeleteCategory;
using FeedInsight.Application.Features.Categories.Commands.UpdateCategory;
using FeedInsight.Application.Features.Categories.Queries.GetCategories;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedInsight.API.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = Role.ProductOwner)] // Only Product Owners manage categories for their Tenant
public class CategoriesController : ApiController
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// GET: api/categories
    /// Retrieves all categories for the currently logged-in Product Owner's company.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _mediator.SendAsync(new GetCategoriesQuery());

        return result.Match(
            categories => OkResponse(categories),
            errors => Problem(errors)
        );
    }

    
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
    {
        var result = await _mediator.SendAsync(command);

        return result.Match(
            categoryId => OkResponse(new { Id = categoryId, Message = "Category created successfully." }),
            errors => Problem(errors)
        );
    }

    
    [HttpPut]
    public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryCommand command)
    {
        var result = await _mediator.SendAsync(command);

        return result.Match(
            success => OkResponse(new { Message = "Category updated successfully." }),
            errors => Problem(errors)
        );
    }

    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var result = await _mediator.SendAsync(new DeleteCategoryCommand(id));

        return result.Match(
            success => OkResponse(new { Message = "Category deleted successfully." }),
            errors => Problem(errors)
        );
    }
}