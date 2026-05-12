using DarwinDeutsch.Maui.Resources.Strings;

namespace DarwinDeutsch.Maui.Pages;

internal sealed record LearningPortalModuleDescriptor(string Key, string Title, string Description)
{
    public static LearningPortalModuleDescriptor Resolve(string? module)
    {
        string key = string.IsNullOrWhiteSpace(module)
            ? "grammar"
            : module.Trim().ToLowerInvariant();

        return key switch
        {
            "expressions" => new(key, AppStrings.LearningPortalItemExpressions, AppStrings.LearningPortalExpressionsDescription),
            "exercises" => new(key, AppStrings.LearningPortalItemExercises, AppStrings.LearningPortalExercisesDescription),
            "courses" => new(key, AppStrings.LearningPortalItemCourses, AppStrings.LearningPortalCoursesDescription),
            "writing-templates" => new(key, AppStrings.LearningPortalItemWritingTemplates, AppStrings.LearningPortalWritingTemplatesDescription),
            "cultural-notes" => new(key, AppStrings.LearningPortalItemCulturalNotes, AppStrings.LearningPortalCulturalNotesDescription),
            "exam-prep" => new(key, AppStrings.LearningPortalItemExamPrep, AppStrings.LearningPortalExamPrepDescription),
            "talk-topics" => new(key, AppStrings.LearningPortalItemTalkTopics, AppStrings.LearningPortalTalkTopicsDescription),
            _ => new("grammar", AppStrings.LearningPortalItemGrammar, AppStrings.LearningPortalGrammarDescription),
        };
    }
}
