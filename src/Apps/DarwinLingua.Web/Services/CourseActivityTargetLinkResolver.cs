using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Services;

public static class CourseActivityTargetLinkResolver
{
    public static string? ResolveHref(CourseLessonActivityBlockModel activity, string currentCourseSlug)
    {
        if (string.Equals(activity.TargetType, "none", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return LearningContentLinkResolver.ResolveHref(activity.TargetType, activity.TargetSlug, currentCourseSlug);
    }
}
