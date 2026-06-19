namespace DarwinLingua.Web.Services;

public static class LearningContentLinkResolver
{
    public static string? ResolveHref(string contentType, string? slug, string? currentCourseSlug = null)
    {
        if (string.IsNullOrWhiteSpace(contentType) || string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        string normalizedType = contentType.Trim().ToLowerInvariant();
        string encodedSlug = Uri.EscapeDataString(slug.Trim());

        return normalizedType switch
        {
            "course-lesson" when !string.IsNullOrWhiteSpace(currentCourseSlug) =>
                $"/courses/{Uri.EscapeDataString(currentCourseSlug.Trim())}/{encodedSlug}",
            "course-lesson" => $"/courses/lessons/{encodedSlug}",
            "grammar-topic" => $"/grammar/{encodedSlug}",
            "word" => $"/words/{encodedSlug}",
            "expression" => $"/expressions/{encodedSlug}",
            "dialogue" => $"/dialogues/{encodedSlug}",
            "talk-topic" => $"/talk-topics/{encodedSlug}",
            "exercise-set" => $"/exercises/sets/{encodedSlug}",
            "exercise" => $"/exercises/{encodedSlug}",
            "roleplay" => $"/roleplays/{encodedSlug}",
            "writing-template" => $"/writing-templates/{encodedSlug}",
            "life-in-germany" => $"/life-in-germany/{encodedSlug}",
            "exam-prep-unit" => $"/exam-prep/{encodedSlug}",
            _ => null,
        };
    }
}
