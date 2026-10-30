using Entity_Framework_Practice.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Entity_Framework_Practice.Controllers;

public static class ControllerExtensions
{
    // Turns a failed service result into a ProblemDetails response with the right status code
    public static ObjectResult ToProblem(this ControllerBase controller, IServiceResult result)
    {
        var (status, title) = result.Status switch
        {
            ResultStatus.NotFound => (StatusCodes.Status404NotFound, "Not found"),
            ResultStatus.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            ResultStatus.Invalid => (StatusCodes.Status400BadRequest, "Invalid request"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error")
        };

        return controller.Problem(detail: result.Error, statusCode: status, title: title);
    }
}
