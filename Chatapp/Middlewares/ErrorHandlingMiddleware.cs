using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebService.Exceptions;

namespace WebService.Middlewares;

public class ErrorHandlingMiddleWare(
    ILogger<ErrorHandlingMiddleWare> logger,
    ProblemDetailsFactory problemDetailsFactory)
    : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next.Invoke(context);
        }
        catch (BadRequestException e)
        {
            LogError(e);
            var result = GetValidationResult(context, "Bad Request", e.Message);
            var actionContext = new ActionContext(context, context.GetRouteData(), new ActionDescriptor());
            await result.ExecuteResultAsync(actionContext);
        }
        catch (UnauthorizedException e)
        {
            LogError(e);
            var result = GetResult(context, StatusCodes.Status401Unauthorized, e.Message);
            var actionContext = new ActionContext(context, context.GetRouteData(), new ActionDescriptor());
            await result.ExecuteResultAsync(actionContext);
        }
        catch (Exception e)
        {
            LogError(e);
            var result = GetResult(context, StatusCodes.Status500InternalServerError, "Something went wrong");
            var actionContext = new ActionContext(context, context.GetRouteData(), new ActionDescriptor());
            await result.ExecuteResultAsync(actionContext);
        }
    }

    private void LogError(Exception exception)
    {
        logger.LogError(
            "An exception occured.\n" +
            "Type: {ExceptionType}" +
            "Message: {ExceptionMessage}\n" +
            "Stack trace:{ExceptionStackTrace}",
            exception.GetType(),
            exception.Message,
            exception.StackTrace
        );
    }

    private ObjectResult GetResult(HttpContext context, int statusCode, string message)
    {
        var problemDetails = problemDetailsFactory.CreateProblemDetails(context, statusCode, detail: message);
        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }

    private ObjectResult GetValidationResult(HttpContext context, string title, string message)
    {
        var errors = new ModelStateDictionary();
        errors.AddModelError(title, message);

        var problemDetails = problemDetailsFactory
            .CreateValidationProblemDetails(context, errors, statusCode: StatusCodes.Status400BadRequest);

        return new ObjectResult(problemDetails);
    }
}