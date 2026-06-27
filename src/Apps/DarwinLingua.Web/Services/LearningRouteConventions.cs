using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Web.Services;

/// <summary>
/// Centralizes learner-facing target-language route conventions.
/// </summary>
public static class LearningRouteConventions
{
    public const string TargetLearningLanguageRouteKey = "targetLearningLanguageCode";
    public const string LearnPrefix = "learn/{" + TargetLearningLanguageRouteKey + "}";

    public const string Browse = LearnPrefix + "/browse";
    public const string Collections = LearnPrefix + "/collections";
    public const string Words = LearnPrefix + "/words";
    public const string Grammar = LearnPrefix + "/grammar";
    public const string Expressions = LearnPrefix + "/expressions";
    public const string Courses = LearnPrefix + "/courses";
    public const string Exercises = LearnPrefix + "/exercises";
    public const string Roleplays = LearnPrefix + "/roleplays";
    public const string TalkTopics = LearnPrefix + "/talk-topics";
    public const string Dialogues = LearnPrefix + "/dialogues";
    public const string ConversationStarters = LearnPrefix + "/conversation-starters";
    public const string EventPreparationPacks = LearnPrefix + "/event-preparation-packs";
    public const string ConversationEvents = LearnPrefix + "/conversation-events";
    public const string OrganizerProfiles = LearnPrefix + "/organizers";
    public const string ExamPrep = LearnPrefix + "/exam-prep";
    public const string WritingTemplates = LearnPrefix + "/writing-templates";
    public const string CountryContextRouteKey = "countryContextCode";
    public const string CountryGuidance = LearnPrefix + "/country-guidance/{" + CountryContextRouteKey + "}";
    public const string Search = LearnPrefix + "/search";
    public const string Recent = LearnPrefix + "/recent";
    public const string Favorites = LearnPrefix + "/favorites";

    public static string ResolveTargetLearningLanguageCode(HttpContext? httpContext)
    {
        if (httpContext?.Items.TryGetValue(TargetLearningLanguageRouteKey, out object? scopedValue) == true
            && scopedValue is not null
            && TargetLearningLanguageCatalog.TryFindActive(scopedValue.ToString() ?? string.Empty, out TargetLearningLanguageDefinition scopedLanguage))
        {
            return scopedLanguage.Code;
        }

        if (httpContext?.Request.RouteValues.TryGetValue(TargetLearningLanguageRouteKey, out object? routeValue) == true
            && routeValue is not null)
        {
            string routeLanguageCode = routeValue.ToString() ?? string.Empty;
            if (TargetLearningLanguageCatalog.TryFindActive(routeLanguageCode, out TargetLearningLanguageDefinition language))
            {
                return language.Code;
            }

            throw new InvalidOperationException(
                $"Inactive or unsupported target learning language route '{routeLanguageCode}' reached learner route resolution.");
        }

        return ContentLanguageRequirements.DefaultTargetLearningLanguageCode;
    }

    public static string ResolveDefaultCountryContextRouteValue(string targetLearningLanguageCode) =>
        CountryContextCatalog.ResolveDefaultActiveCode(targetLearningLanguageCode).ToLowerInvariant();
}
