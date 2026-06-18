using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class SyncLearningPortalLocalizedSnapshot20260616 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE "WritingTemplates" ADD COLUMN IF NOT EXISTS "ExplanationTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "WritingTemplates" ADD COLUMN IF NOT EXISTS "LinkedCourseLessonSlugsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "WritingTemplates" ADD COLUMN IF NOT EXISTS "SampleFilledVersionTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "WritingTemplates" ADD COLUMN IF NOT EXISTS "ShortDescriptionTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "WritingTemplates" ADD COLUMN IF NOT EXISTS "SituationTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "WritingTemplates" ADD COLUMN IF NOT EXISTS "TemplateTextTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "WritingTemplates" ADD COLUMN IF NOT EXISTS "TitleTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "ExerciseSets" ADD COLUMN IF NOT EXISTS "DescriptionTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "ExerciseSets" ADD COLUMN IF NOT EXISTS "TitleTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "Exercises" ADD COLUMN IF NOT EXISTS "CommonMistakeNoteTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "Exercises" ADD COLUMN IF NOT EXISTS "CorrectExplanationTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "Exercises" ADD COLUMN IF NOT EXISTS "HintTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "Exercises" ADD COLUMN IF NOT EXISTS "IncorrectExplanationTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "Exercises" ADD COLUMN IF NOT EXISTS "InstructionTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "Exercises" ADD COLUMN IF NOT EXISTS "TitleTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "ExamProfiles" ADD COLUMN IF NOT EXISTS "DescriptionTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "ExamProfiles" ADD COLUMN IF NOT EXISTS "DisplayNameTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "ExamPrepUnits" ADD COLUMN IF NOT EXISTS "ChecklistTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "ExamPrepUnits" ADD COLUMN IF NOT EXISTS "ExplanationTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "ExamPrepUnits" ADD COLUMN IF NOT EXISTS "LinkedRoleplaySlugsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "ExamPrepUnits" ADD COLUMN IF NOT EXISTS "ShortDescriptionTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "ExamPrepUnits" ADD COLUMN IF NOT EXISTS "StrategyNotesTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "ExamPrepUnits" ADD COLUMN IF NOT EXISTS "TitleTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CulturalNotes" ADD COLUMN IF NOT EXISTS "ContextTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CulturalNotes" ADD COLUMN IF NOT EXISTS "DoNotesTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CulturalNotes" ADD COLUMN IF NOT EXISTS "DontNotesTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CulturalNotes" ADD COLUMN IF NOT EXISTS "ExamplesTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CulturalNotes" ADD COLUMN IF NOT EXISTS "SectionsTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CulturalNotes" ADD COLUMN IF NOT EXISTS "SensitivityWarningTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CulturalNotes" ADD COLUMN IF NOT EXISTS "ShortDescriptionTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CulturalNotes" ADD COLUMN IF NOT EXISTS "TitleTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CoursePaths" ADD COLUMN IF NOT EXISTS "DescriptionTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CoursePaths" ADD COLUMN IF NOT EXISTS "TitleTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CourseModules" ADD COLUMN IF NOT EXISTS "DescriptionTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CourseModules" ADD COLUMN IF NOT EXISTS "TitleTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CourseLessons" ADD COLUMN IF NOT EXISTS "ActivityBlocksJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CourseLessons" ADD COLUMN IF NOT EXISTS "HomeworkTaskTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CourseLessons" ADD COLUMN IF NOT EXISTS "LearningGoalsTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CourseLessons" ADD COLUMN IF NOT EXISTS "NarrativeTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CourseLessons" ADD COLUMN IF NOT EXISTS "ReviewSummaryTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CourseLessons" ADD COLUMN IF NOT EXISTS "ShortDescriptionTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CourseLessons" ADD COLUMN IF NOT EXISTS "TitleTranslationsJson" text NOT NULL DEFAULT '[]';
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE "CourseLessons" DROP COLUMN IF EXISTS "TitleTranslationsJson";
            ALTER TABLE "CourseLessons" DROP COLUMN IF EXISTS "ShortDescriptionTranslationsJson";
            ALTER TABLE "CourseLessons" DROP COLUMN IF EXISTS "ReviewSummaryTranslationsJson";
            ALTER TABLE "CourseLessons" DROP COLUMN IF EXISTS "NarrativeTranslationsJson";
            ALTER TABLE "CourseLessons" DROP COLUMN IF EXISTS "LearningGoalsTranslationsJson";
            ALTER TABLE "CourseLessons" DROP COLUMN IF EXISTS "HomeworkTaskTranslationsJson";
            ALTER TABLE "CourseLessons" DROP COLUMN IF EXISTS "ActivityBlocksJson";
            ALTER TABLE "CourseModules" DROP COLUMN IF EXISTS "TitleTranslationsJson";
            ALTER TABLE "CourseModules" DROP COLUMN IF EXISTS "DescriptionTranslationsJson";
            ALTER TABLE "CoursePaths" DROP COLUMN IF EXISTS "TitleTranslationsJson";
            ALTER TABLE "CoursePaths" DROP COLUMN IF EXISTS "DescriptionTranslationsJson";
            ALTER TABLE "CulturalNotes" DROP COLUMN IF EXISTS "TitleTranslationsJson";
            ALTER TABLE "CulturalNotes" DROP COLUMN IF EXISTS "ShortDescriptionTranslationsJson";
            ALTER TABLE "CulturalNotes" DROP COLUMN IF EXISTS "SensitivityWarningTranslationsJson";
            ALTER TABLE "CulturalNotes" DROP COLUMN IF EXISTS "SectionsTranslationsJson";
            ALTER TABLE "CulturalNotes" DROP COLUMN IF EXISTS "ExamplesTranslationsJson";
            ALTER TABLE "CulturalNotes" DROP COLUMN IF EXISTS "DontNotesTranslationsJson";
            ALTER TABLE "CulturalNotes" DROP COLUMN IF EXISTS "DoNotesTranslationsJson";
            ALTER TABLE "CulturalNotes" DROP COLUMN IF EXISTS "ContextTranslationsJson";
            ALTER TABLE "ExamPrepUnits" DROP COLUMN IF EXISTS "TitleTranslationsJson";
            ALTER TABLE "ExamPrepUnits" DROP COLUMN IF EXISTS "StrategyNotesTranslationsJson";
            ALTER TABLE "ExamPrepUnits" DROP COLUMN IF EXISTS "ShortDescriptionTranslationsJson";
            ALTER TABLE "ExamPrepUnits" DROP COLUMN IF EXISTS "LinkedRoleplaySlugsJson";
            ALTER TABLE "ExamPrepUnits" DROP COLUMN IF EXISTS "ExplanationTranslationsJson";
            ALTER TABLE "ExamPrepUnits" DROP COLUMN IF EXISTS "ChecklistTranslationsJson";
            ALTER TABLE "ExamProfiles" DROP COLUMN IF EXISTS "DisplayNameTranslationsJson";
            ALTER TABLE "ExamProfiles" DROP COLUMN IF EXISTS "DescriptionTranslationsJson";
            ALTER TABLE "Exercises" DROP COLUMN IF EXISTS "TitleTranslationsJson";
            ALTER TABLE "Exercises" DROP COLUMN IF EXISTS "InstructionTranslationsJson";
            ALTER TABLE "Exercises" DROP COLUMN IF EXISTS "IncorrectExplanationTranslationsJson";
            ALTER TABLE "Exercises" DROP COLUMN IF EXISTS "HintTranslationsJson";
            ALTER TABLE "Exercises" DROP COLUMN IF EXISTS "CorrectExplanationTranslationsJson";
            ALTER TABLE "Exercises" DROP COLUMN IF EXISTS "CommonMistakeNoteTranslationsJson";
            ALTER TABLE "ExerciseSets" DROP COLUMN IF EXISTS "TitleTranslationsJson";
            ALTER TABLE "ExerciseSets" DROP COLUMN IF EXISTS "DescriptionTranslationsJson";
            ALTER TABLE "WritingTemplates" DROP COLUMN IF EXISTS "TitleTranslationsJson";
            ALTER TABLE "WritingTemplates" DROP COLUMN IF EXISTS "TemplateTextTranslationsJson";
            ALTER TABLE "WritingTemplates" DROP COLUMN IF EXISTS "SituationTranslationsJson";
            ALTER TABLE "WritingTemplates" DROP COLUMN IF EXISTS "ShortDescriptionTranslationsJson";
            ALTER TABLE "WritingTemplates" DROP COLUMN IF EXISTS "SampleFilledVersionTranslationsJson";
            ALTER TABLE "WritingTemplates" DROP COLUMN IF EXISTS "LinkedCourseLessonSlugsJson";
            ALTER TABLE "WritingTemplates" DROP COLUMN IF EXISTS "ExplanationTranslationsJson";
            """);
    }
}
