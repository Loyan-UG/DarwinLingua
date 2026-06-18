using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Services;

public static class CourseActivityTargetLinkResolver
{
    public static string? ResolveHref(CourseLessonActivityBlockModel activity, string currentCourseSlug)
    {
        string targetType = activity.TargetType.Trim().ToLowerInvariant();
        string? targetSlug = string.IsNullOrWhiteSpace(activity.TargetSlug)
            ? null
            : Uri.EscapeDataString(activity.TargetSlug.Trim());

        return targetType switch
        {
            "none" => null,
            "course-lesson" when targetSlug is not null => $"/courses/{Uri.EscapeDataString(currentCourseSlug)}/{targetSlug}",
            "grammar-topic" when targetSlug is not null => $"/grammar/{targetSlug}",
            "expression" when targetSlug is not null => $"/expressions/{targetSlug}",
            "dialogue" when targetSlug is not null => $"/dialogues/{targetSlug}",
            "talk-topic" when targetSlug is not null => $"/talk-topics/{targetSlug}",
            "exercise-set" when targetSlug is not null => $"/exercises/sets/{targetSlug}",
            "exercise" when targetSlug is not null => $"/exercises/{targetSlug}",
            "roleplay" when targetSlug is not null => $"/roleplays/{targetSlug}",
            "writing-template" when targetSlug is not null => $"/writing-templates/{targetSlug}",
            "life-in-germany" when targetSlug is not null => $"/life-in-germany/{targetSlug}",
            "exam-prep-unit" when targetSlug is not null => $"/exam-prep/{targetSlug}",
            _ => null,
        };
    }
}
