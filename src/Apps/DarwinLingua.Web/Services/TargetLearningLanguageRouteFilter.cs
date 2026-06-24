using DarwinLingua.SharedKernel.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DarwinLingua.Web.Services;

/// <summary>
/// Rejects learner routes that ask for an inactive target learning language.
/// </summary>
public sealed class TargetLearningLanguageRouteFilter : IAsyncActionFilter
{
    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        if (!context.RouteData.Values.TryGetValue(LearningRouteConventions.TargetLearningLanguageRouteKey, out object? value)
            || value is null)
        {
            return next();
        }

        string routeLanguageCode = value.ToString() ?? string.Empty;
        if (TargetLearningLanguageCatalog.TryFindActive(routeLanguageCode, out TargetLearningLanguageDefinition language))
        {
            context.HttpContext.Items[LearningRouteConventions.TargetLearningLanguageRouteKey] = language.Code;
            return next();
        }

        context.Result = new NotFoundResult();
        return Task.CompletedTask;
    }
}
