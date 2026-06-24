using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTargetLearningLanguageScopeToLearningContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WritingTemplates_CefrLevel_Category_Register",
                table: "WritingTemplates");

            migrationBuilder.DropIndex(
                name: "IX_WritingTemplates_Slug",
                table: "WritingTemplates");

            migrationBuilder.DropIndex(
                name: "IX_TalkTopics_CefrLevel_ContentType_Category",
                table: "TalkTopics");

            migrationBuilder.DropIndex(
                name: "IX_TalkTopics_Slug",
                table: "TalkTopics");

            migrationBuilder.DropIndex(
                name: "IX_RoleplayScenarios_CefrLevel_Category_TaskType_InteractionMode_Register",
                table: "RoleplayScenarios");

            migrationBuilder.DropIndex(
                name: "IX_RoleplayScenarios_Slug",
                table: "RoleplayScenarios");

            migrationBuilder.DropIndex(
                name: "IX_GrammarTopics_CefrLevel_GrammarCategory",
                table: "GrammarTopics");

            migrationBuilder.DropIndex(
                name: "IX_GrammarTopics_Slug",
                table: "GrammarTopics");

            migrationBuilder.DropIndex(
                name: "IX_ExpressionEntries_CefrLevel_ExpressionType_Register_Category",
                table: "ExpressionEntries");

            migrationBuilder.DropIndex(
                name: "IX_ExpressionEntries_Slug",
                table: "ExpressionEntries");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseSets_OwnerType_OwnerSlug",
                table: "ExerciseSets");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseSets_Slug",
                table: "ExerciseSets");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_CefrLevel_ExerciseType_TargetSkill",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_OwnerType_OwnerSlug",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_Slug",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_ExamProfiles_Key",
                table: "ExamProfiles");

            migrationBuilder.DropIndex(
                name: "IX_ExamPrepUnits_ExamProfileKey_CefrLevel_ExamSection",
                table: "ExamPrepUnits");

            migrationBuilder.DropIndex(
                name: "IX_ExamPrepUnits_Slug",
                table: "ExamPrepUnits");

            migrationBuilder.DropIndex(
                name: "IX_DialogueLessons_Category",
                table: "DialogueLessons");

            migrationBuilder.DropIndex(
                name: "IX_DialogueLessons_CefrLevel",
                table: "DialogueLessons");

            migrationBuilder.DropIndex(
                name: "IX_DialogueLessons_InteractionMode",
                table: "DialogueLessons");

            migrationBuilder.DropIndex(
                name: "IX_DialogueLessons_Register",
                table: "DialogueLessons");

            migrationBuilder.DropIndex(
                name: "IX_DialogueLessons_Slug",
                table: "DialogueLessons");

            migrationBuilder.DropIndex(
                name: "IX_DialogueLessons_TaskType",
                table: "DialogueLessons");

            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_CountryGuidanceNotes_CefrLevel_Category";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_CountryGuidanceNotes_Slug";""");

            migrationBuilder.DropIndex(
                name: "IX_CoursePaths_Slug",
                table: "CoursePaths");

            migrationBuilder.DropIndex(
                name: "IX_CourseModules_Slug",
                table: "CourseModules");

            migrationBuilder.DropIndex(
                name: "IX_CourseLessons_CoursePathSlug_ModuleSlug",
                table: "CourseLessons");

            migrationBuilder.DropIndex(
                name: "IX_CourseLessons_Slug",
                table: "CourseLessons");

            migrationBuilder.AddColumn<string>(
                name: "TargetLearningLanguageCode",
                table: "WritingTemplates",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "de");

            migrationBuilder.AddColumn<string>(
                name: "TargetLearningLanguageCode",
                table: "TalkTopics",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "de");

            migrationBuilder.AddColumn<string>(
                name: "TargetLearningLanguageCode",
                table: "RoleplayScenarios",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "de");

            migrationBuilder.AddColumn<string>(
                name: "TargetLearningLanguageCode",
                table: "GrammarTopics",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "de");

            migrationBuilder.AddColumn<string>(
                name: "TargetLearningLanguageCode",
                table: "ExpressionEntries",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "de");

            migrationBuilder.AddColumn<string>(
                name: "TargetLearningLanguageCode",
                table: "ExerciseSets",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "de");

            migrationBuilder.AddColumn<string>(
                name: "TargetLearningLanguageCode",
                table: "Exercises",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "de");

            migrationBuilder.AddColumn<string>(
                name: "TargetLearningLanguageCode",
                table: "ExamProfiles",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "de");

            migrationBuilder.AddColumn<string>(
                name: "TargetLearningLanguageCode",
                table: "ExamPrepUnits",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "de");

            migrationBuilder.AddColumn<string>(
                name: "TargetLearningLanguageCode",
                table: "DialogueLessons",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "de");

            migrationBuilder.AddColumn<string>(
                name: "TargetLearningLanguageCode",
                table: "CountryGuidanceNotes",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "de");

            migrationBuilder.AddColumn<string>(
                name: "TargetLearningLanguageCode",
                table: "CoursePaths",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "de");

            migrationBuilder.AddColumn<string>(
                name: "TargetLearningLanguageCode",
                table: "CourseModules",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "de");

            migrationBuilder.AddColumn<string>(
                name: "TargetLearningLanguageCode",
                table: "CourseLessons",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "de");

            migrationBuilder.CreateIndex(
                name: "IX_WritingTemplates_TargetLearningLanguageCode_CefrLevel_Category_Register",
                table: "WritingTemplates",
                columns: new[] { "TargetLearningLanguageCode", "CefrLevel", "Category", "Register" });

            migrationBuilder.CreateIndex(
                name: "IX_WritingTemplates_TargetLearningLanguageCode_Slug",
                table: "WritingTemplates",
                columns: new[] { "TargetLearningLanguageCode", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TalkTopics_TargetLearningLanguageCode_CefrLevel_ContentType_Category",
                table: "TalkTopics",
                columns: new[] { "TargetLearningLanguageCode", "CefrLevel", "ContentType", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_TalkTopics_TargetLearningLanguageCode_Slug",
                table: "TalkTopics",
                columns: new[] { "TargetLearningLanguageCode", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleplayScenarios_TargetLearningLanguageCode_CefrLevel_Category_TaskType_InteractionMode_Register",
                table: "RoleplayScenarios",
                columns: new[] { "TargetLearningLanguageCode", "CefrLevel", "Category", "TaskType", "InteractionMode", "Register" });

            migrationBuilder.CreateIndex(
                name: "IX_RoleplayScenarios_TargetLearningLanguageCode_Slug",
                table: "RoleplayScenarios",
                columns: new[] { "TargetLearningLanguageCode", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarTopics_TargetLearningLanguageCode_CefrLevel_GrammarCategory",
                table: "GrammarTopics",
                columns: new[] { "TargetLearningLanguageCode", "CefrLevel", "GrammarCategory" });

            migrationBuilder.CreateIndex(
                name: "IX_GrammarTopics_TargetLearningLanguageCode_Slug",
                table: "GrammarTopics",
                columns: new[] { "TargetLearningLanguageCode", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpressionEntries_TargetLearningLanguageCode_CefrLevel_ExpressionType_Register_Category",
                table: "ExpressionEntries",
                columns: new[] { "TargetLearningLanguageCode", "CefrLevel", "ExpressionType", "Register", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_ExpressionEntries_TargetLearningLanguageCode_Slug",
                table: "ExpressionEntries",
                columns: new[] { "TargetLearningLanguageCode", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSets_TargetLearningLanguageCode_OwnerType_OwnerSlug",
                table: "ExerciseSets",
                columns: new[] { "TargetLearningLanguageCode", "OwnerType", "OwnerSlug" });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSets_TargetLearningLanguageCode_Slug",
                table: "ExerciseSets",
                columns: new[] { "TargetLearningLanguageCode", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_TargetLearningLanguageCode_CefrLevel_ExerciseType_TargetSkill",
                table: "Exercises",
                columns: new[] { "TargetLearningLanguageCode", "CefrLevel", "ExerciseType", "TargetSkill" });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_TargetLearningLanguageCode_OwnerType_OwnerSlug",
                table: "Exercises",
                columns: new[] { "TargetLearningLanguageCode", "OwnerType", "OwnerSlug" });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_TargetLearningLanguageCode_Slug",
                table: "Exercises",
                columns: new[] { "TargetLearningLanguageCode", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamProfiles_TargetLearningLanguageCode_Key",
                table: "ExamProfiles",
                columns: new[] { "TargetLearningLanguageCode", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamPrepUnits_TargetLearningLanguageCode_ExamProfileKey_CefrLevel_ExamSection",
                table: "ExamPrepUnits",
                columns: new[] { "TargetLearningLanguageCode", "ExamProfileKey", "CefrLevel", "ExamSection" });

            migrationBuilder.CreateIndex(
                name: "IX_ExamPrepUnits_TargetLearningLanguageCode_Slug",
                table: "ExamPrepUnits",
                columns: new[] { "TargetLearningLanguageCode", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueLessons_TargetLanguage_Category",
                table: "DialogueLessons",
                columns: new[] { "TargetLearningLanguageCode", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_DialogueLessons_TargetLanguage_CefrLevel",
                table: "DialogueLessons",
                columns: new[] { "TargetLearningLanguageCode", "CefrLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_DialogueLessons_TargetLanguage_InteractionMode",
                table: "DialogueLessons",
                columns: new[] { "TargetLearningLanguageCode", "InteractionMode" });

            migrationBuilder.CreateIndex(
                name: "IX_DialogueLessons_TargetLanguage_Register",
                table: "DialogueLessons",
                columns: new[] { "TargetLearningLanguageCode", "Register" });

            migrationBuilder.CreateIndex(
                name: "IX_DialogueLessons_TargetLanguage_TaskType",
                table: "DialogueLessons",
                columns: new[] { "TargetLearningLanguageCode", "TaskType" });

            migrationBuilder.CreateIndex(
                name: "IX_DialogueLessons_TargetLearningLanguageCode_Slug",
                table: "DialogueLessons",
                columns: new[] { "TargetLearningLanguageCode", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CountryGuidanceNotes_TargetLearningLanguageCode_CefrLevel_Category",
                table: "CountryGuidanceNotes",
                columns: new[] { "TargetLearningLanguageCode", "CefrLevel", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_CountryGuidanceNotes_TargetLearningLanguageCode_Slug",
                table: "CountryGuidanceNotes",
                columns: new[] { "TargetLearningLanguageCode", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoursePaths_TargetLearningLanguageCode_Slug",
                table: "CoursePaths",
                columns: new[] { "TargetLearningLanguageCode", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseModules_TargetLearningLanguageCode_Slug",
                table: "CourseModules",
                columns: new[] { "TargetLearningLanguageCode", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseLessons_TargetLearningLanguageCode_CoursePathSlug_ModuleSlug",
                table: "CourseLessons",
                columns: new[] { "TargetLearningLanguageCode", "CoursePathSlug", "ModuleSlug" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseLessons_TargetLearningLanguageCode_Slug",
                table: "CourseLessons",
                columns: new[] { "TargetLearningLanguageCode", "Slug" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WritingTemplates_TargetLearningLanguageCode_CefrLevel_Category_Register",
                table: "WritingTemplates");

            migrationBuilder.DropIndex(
                name: "IX_WritingTemplates_TargetLearningLanguageCode_Slug",
                table: "WritingTemplates");

            migrationBuilder.DropIndex(
                name: "IX_TalkTopics_TargetLearningLanguageCode_CefrLevel_ContentType_Category",
                table: "TalkTopics");

            migrationBuilder.DropIndex(
                name: "IX_TalkTopics_TargetLearningLanguageCode_Slug",
                table: "TalkTopics");

            migrationBuilder.DropIndex(
                name: "IX_RoleplayScenarios_TargetLearningLanguageCode_CefrLevel_Category_TaskType_InteractionMode_Register",
                table: "RoleplayScenarios");

            migrationBuilder.DropIndex(
                name: "IX_RoleplayScenarios_TargetLearningLanguageCode_Slug",
                table: "RoleplayScenarios");

            migrationBuilder.DropIndex(
                name: "IX_GrammarTopics_TargetLearningLanguageCode_CefrLevel_GrammarCategory",
                table: "GrammarTopics");

            migrationBuilder.DropIndex(
                name: "IX_GrammarTopics_TargetLearningLanguageCode_Slug",
                table: "GrammarTopics");

            migrationBuilder.DropIndex(
                name: "IX_ExpressionEntries_TargetLearningLanguageCode_CefrLevel_ExpressionType_Register_Category",
                table: "ExpressionEntries");

            migrationBuilder.DropIndex(
                name: "IX_ExpressionEntries_TargetLearningLanguageCode_Slug",
                table: "ExpressionEntries");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseSets_TargetLearningLanguageCode_OwnerType_OwnerSlug",
                table: "ExerciseSets");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseSets_TargetLearningLanguageCode_Slug",
                table: "ExerciseSets");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_TargetLearningLanguageCode_CefrLevel_ExerciseType_TargetSkill",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_TargetLearningLanguageCode_OwnerType_OwnerSlug",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_TargetLearningLanguageCode_Slug",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_ExamProfiles_TargetLearningLanguageCode_Key",
                table: "ExamProfiles");

            migrationBuilder.DropIndex(
                name: "IX_ExamPrepUnits_TargetLearningLanguageCode_ExamProfileKey_CefrLevel_ExamSection",
                table: "ExamPrepUnits");

            migrationBuilder.DropIndex(
                name: "IX_ExamPrepUnits_TargetLearningLanguageCode_Slug",
                table: "ExamPrepUnits");

            migrationBuilder.DropIndex(
                name: "IX_DialogueLessons_TargetLanguage_Category",
                table: "DialogueLessons");

            migrationBuilder.DropIndex(
                name: "IX_DialogueLessons_TargetLanguage_CefrLevel",
                table: "DialogueLessons");

            migrationBuilder.DropIndex(
                name: "IX_DialogueLessons_TargetLanguage_InteractionMode",
                table: "DialogueLessons");

            migrationBuilder.DropIndex(
                name: "IX_DialogueLessons_TargetLanguage_Register",
                table: "DialogueLessons");

            migrationBuilder.DropIndex(
                name: "IX_DialogueLessons_TargetLanguage_TaskType",
                table: "DialogueLessons");

            migrationBuilder.DropIndex(
                name: "IX_DialogueLessons_TargetLearningLanguageCode_Slug",
                table: "DialogueLessons");

            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_CountryGuidanceNotes_TargetLearningLanguageCode_CefrLevel_Category";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_CountryGuidanceNotes_TargetLearningLanguageCode_Slug";""");

            migrationBuilder.DropIndex(
                name: "IX_CoursePaths_TargetLearningLanguageCode_Slug",
                table: "CoursePaths");

            migrationBuilder.DropIndex(
                name: "IX_CourseModules_TargetLearningLanguageCode_Slug",
                table: "CourseModules");

            migrationBuilder.DropIndex(
                name: "IX_CourseLessons_TargetLearningLanguageCode_CoursePathSlug_ModuleSlug",
                table: "CourseLessons");

            migrationBuilder.DropIndex(
                name: "IX_CourseLessons_TargetLearningLanguageCode_Slug",
                table: "CourseLessons");

            migrationBuilder.DropColumn(
                name: "TargetLearningLanguageCode",
                table: "WritingTemplates");

            migrationBuilder.DropColumn(
                name: "TargetLearningLanguageCode",
                table: "TalkTopics");

            migrationBuilder.DropColumn(
                name: "TargetLearningLanguageCode",
                table: "RoleplayScenarios");

            migrationBuilder.DropColumn(
                name: "TargetLearningLanguageCode",
                table: "GrammarTopics");

            migrationBuilder.DropColumn(
                name: "TargetLearningLanguageCode",
                table: "ExpressionEntries");

            migrationBuilder.DropColumn(
                name: "TargetLearningLanguageCode",
                table: "ExerciseSets");

            migrationBuilder.DropColumn(
                name: "TargetLearningLanguageCode",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "TargetLearningLanguageCode",
                table: "ExamProfiles");

            migrationBuilder.DropColumn(
                name: "TargetLearningLanguageCode",
                table: "ExamPrepUnits");

            migrationBuilder.DropColumn(
                name: "TargetLearningLanguageCode",
                table: "DialogueLessons");

            migrationBuilder.DropColumn(
                name: "TargetLearningLanguageCode",
                table: "CountryGuidanceNotes");

            migrationBuilder.DropColumn(
                name: "TargetLearningLanguageCode",
                table: "CoursePaths");

            migrationBuilder.DropColumn(
                name: "TargetLearningLanguageCode",
                table: "CourseModules");

            migrationBuilder.DropColumn(
                name: "TargetLearningLanguageCode",
                table: "CourseLessons");

            migrationBuilder.CreateIndex(
                name: "IX_WritingTemplates_CefrLevel_Category_Register",
                table: "WritingTemplates",
                columns: new[] { "CefrLevel", "Category", "Register" });

            migrationBuilder.CreateIndex(
                name: "IX_WritingTemplates_Slug",
                table: "WritingTemplates",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TalkTopics_CefrLevel_ContentType_Category",
                table: "TalkTopics",
                columns: new[] { "CefrLevel", "ContentType", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_TalkTopics_Slug",
                table: "TalkTopics",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleplayScenarios_CefrLevel_Category_TaskType_InteractionMode_Register",
                table: "RoleplayScenarios",
                columns: new[] { "CefrLevel", "Category", "TaskType", "InteractionMode", "Register" });

            migrationBuilder.CreateIndex(
                name: "IX_RoleplayScenarios_Slug",
                table: "RoleplayScenarios",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarTopics_CefrLevel_GrammarCategory",
                table: "GrammarTopics",
                columns: new[] { "CefrLevel", "GrammarCategory" });

            migrationBuilder.CreateIndex(
                name: "IX_GrammarTopics_Slug",
                table: "GrammarTopics",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpressionEntries_CefrLevel_ExpressionType_Register_Category",
                table: "ExpressionEntries",
                columns: new[] { "CefrLevel", "ExpressionType", "Register", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_ExpressionEntries_Slug",
                table: "ExpressionEntries",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSets_OwnerType_OwnerSlug",
                table: "ExerciseSets",
                columns: new[] { "OwnerType", "OwnerSlug" });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSets_Slug",
                table: "ExerciseSets",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_CefrLevel_ExerciseType_TargetSkill",
                table: "Exercises",
                columns: new[] { "CefrLevel", "ExerciseType", "TargetSkill" });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_OwnerType_OwnerSlug",
                table: "Exercises",
                columns: new[] { "OwnerType", "OwnerSlug" });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Slug",
                table: "Exercises",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamProfiles_Key",
                table: "ExamProfiles",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamPrepUnits_ExamProfileKey_CefrLevel_ExamSection",
                table: "ExamPrepUnits",
                columns: new[] { "ExamProfileKey", "CefrLevel", "ExamSection" });

            migrationBuilder.CreateIndex(
                name: "IX_ExamPrepUnits_Slug",
                table: "ExamPrepUnits",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueLessons_Category",
                table: "DialogueLessons",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_DialogueLessons_CefrLevel",
                table: "DialogueLessons",
                column: "CefrLevel");

            migrationBuilder.CreateIndex(
                name: "IX_DialogueLessons_InteractionMode",
                table: "DialogueLessons",
                column: "InteractionMode");

            migrationBuilder.CreateIndex(
                name: "IX_DialogueLessons_Register",
                table: "DialogueLessons",
                column: "Register");

            migrationBuilder.CreateIndex(
                name: "IX_DialogueLessons_Slug",
                table: "DialogueLessons",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueLessons_TaskType",
                table: "DialogueLessons",
                column: "TaskType");

            migrationBuilder.CreateIndex(
                name: "IX_CountryGuidanceNotes_CefrLevel_Category",
                table: "CountryGuidanceNotes",
                columns: new[] { "CefrLevel", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_CountryGuidanceNotes_Slug",
                table: "CountryGuidanceNotes",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoursePaths_Slug",
                table: "CoursePaths",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseModules_Slug",
                table: "CourseModules",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseLessons_CoursePathSlug_ModuleSlug",
                table: "CourseLessons",
                columns: new[] { "CoursePathSlug", "ModuleSlug" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseLessons_Slug",
                table: "CourseLessons",
                column: "Slug",
                unique: true);
        }
    }
}
