namespace DarwinLingua.Web.Services;

public static class LearningContentLinkResolver
{
    public static string? ResolveHref(
        string contentType,
        string? slug,
        string targetLearningLanguageCode,
        string? currentCourseSlug = null,
        string? countryContextCode = null)
    {
        if (string.IsNullOrWhiteSpace(contentType)
            || string.IsNullOrWhiteSpace(slug)
            || string.IsNullOrWhiteSpace(targetLearningLanguageCode))
        {
            return null;
        }

        string normalizedType = contentType.Trim().ToLowerInvariant();
        string encodedSlug = Uri.EscapeDataString(slug.Trim());
        string encodedTargetLearningLanguageCode = Uri.EscapeDataString(targetLearningLanguageCode.Trim().ToLowerInvariant());
        string normalizedTargetLearningLanguageCode = targetLearningLanguageCode.Trim().ToLowerInvariant();
        string encodedCountryContextCode = Uri.EscapeDataString(
            string.IsNullOrWhiteSpace(countryContextCode)
                ? LearningRouteConventions.ResolveDefaultCountryContextRouteValue(normalizedTargetLearningLanguageCode)
                : countryContextCode.Trim().ToLowerInvariant());
        string learnPrefix = "/learn/" + encodedTargetLearningLanguageCode;

        return normalizedType switch
        {
            "course-lesson" when !string.IsNullOrWhiteSpace(currentCourseSlug) =>
                $"{learnPrefix}/courses/{Uri.EscapeDataString(currentCourseSlug.Trim())}/{encodedSlug}",
            "course-lesson" => $"{learnPrefix}/courses/lessons/{encodedSlug}",
            "grammar-topic" => $"{learnPrefix}/grammar/{encodedSlug}",
            "word" => $"{learnPrefix}/words/{encodedSlug}",
            "expression" => $"{learnPrefix}/expressions/{encodedSlug}",
            "dialogue" => $"{learnPrefix}/dialogues/{encodedSlug}",
            "talk-topic" => $"{learnPrefix}/talk-topics/{encodedSlug}",
            "exercise-set" => $"{learnPrefix}/exercises/sets/{encodedSlug}",
            "exercise" => $"{learnPrefix}/exercises/{encodedSlug}",
            "roleplay" => $"{learnPrefix}/roleplays/{encodedSlug}",
            "writing-template" => $"{learnPrefix}/writing-templates/{encodedSlug}",
            "country-guidance" => $"{learnPrefix}/country-guidance/{encodedCountryContextCode}/{encodedSlug}",
            "exam-prep-unit" => $"{learnPrefix}/exam-prep/{encodedSlug}",
            _ => null,
        };
    }
}
