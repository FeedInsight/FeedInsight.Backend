using ErrorOr;
using FeedInsight.API.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FeedInsight.API.Controllers;

[ApiController]
public class ApiController : ControllerBase
{
    /// <summary>
    /// Wraps a single resource in the standard ApiResponse envelope.
    /// </summary>
    protected IActionResult OkResponse<T>(T data, object? meta = null)
    {
        // Inject a default meta object if none is provided
        meta ??= new { timestamp = DateTime.UtcNow, version = "1.0" };

        return Ok(new ApiResponse<T>(data, meta));
    }


    /// <summary>
    /// Wraps a list of items in the standard PaginatedResponse envelope.
    /// </summary>
    protected IActionResult OkPaginated<T>(IEnumerable<T> data, int currentPage, int pageSize, int totalItems, object? meta = null)
    {
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var pagination = new PaginationMetadata(
            currentPage,
            pageSize,
            totalItems,
            totalPages,
            currentPage < totalPages,
            currentPage > 1
        );

        meta ??= new { timestamp = DateTime.UtcNow, version = "1.0" };

        return Ok(new PaginatedResponse<T>(data, pagination, meta));
    }


    /// <summary>
    /// Maps a list of ErrorOr errors to the appropriate RFC 7807 ProblemDetails response.
    /// </summary>
    protected IActionResult Problem(List<Error> errors)
    {
        if (errors.Count == 0)
        {
            return Problem();
        }

        // 1. Handle Validation Errors: Return a 400 Bad Request with all errors mapped
        if (errors.Any(e => e.Type == ErrorType.Validation))
        {
            var modelState = new ModelStateDictionary();

            foreach (var error in errors.Where(e => e.Type == ErrorType.Validation))
            {
                modelState.AddModelError(error.Code, error.Description);
            }

            return ValidationProblem(modelState);
        }

        // 2. Handle Non-Validation Errors
        var firstError = errors[0];

        var statusCode = firstError.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Failure => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        // Pass the Error Code into the extensions dictionary so the frontend can read it programmatically!
        return Problem(
            statusCode: statusCode,
            title: firstError.Description,
            extensions: new Dictionary<string, object?>
            {
                { "errorCode", firstError.Code }
            });
    }
}
