using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FeedInsight.API.Controllers;

public class ApiController : ControllerBase
{
    protected IActionResult Problem(List<Error> errors)
    {
        // 1. If we have any validation errors, return 400 Bad Request
        if (errors.Any(e => e.Type == ErrorType.Validation))
        {
            var modelState = new ModelStateDictionary();
            foreach (var error in errors.Where(e => e.Type == ErrorType.Validation))
            {
                modelState.AddModelError(error.Code, error.Description);
            }
            return ValidationProblem(modelState);
        }

        // 2. Handle the first non-validation error
        var firstError = errors[0];

        var statusCode = firstError.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Failure => StatusCodes.Status400BadRequest,
            ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
            ErrorType.Validation => StatusCodes.Status400BadRequest, // Should be caught by the block above
            _ => StatusCodes.Status500InternalServerError
        };

        return Problem(
        statusCode: statusCode,
        title: firstError.Description,
        extensions: new Dictionary<string, object?> { { "errorCode", firstError.Code } }); // Include Code!
    }
}
