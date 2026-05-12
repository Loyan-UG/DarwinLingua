using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUnifiedLearningSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            migrationBuilder.Sql("""CREATE EXTENSION IF NOT EXISTS pg_trgm;""");

            CreateTrigramIndex(migrationBuilder, "IX_WordEntries_Lemma_Trgm", "WordEntries", "Lemma");
            CreateTrigramIndex(migrationBuilder, "IX_WordEntries_NormalizedLemma_Trgm", "WordEntries", "NormalizedLemma");
            CreateTrigramIndex(migrationBuilder, "IX_GrammarTopics_Title_Trgm", "GrammarTopics", "Title");
            CreateTrigramIndex(migrationBuilder, "IX_GrammarTopics_Description_Trgm", "GrammarTopics", "ShortDescription");
            CreateTrigramIndex(migrationBuilder, "IX_GrammarTopics_Slug_Trgm", "GrammarTopics", "Slug");
            CreateTrigramIndex(migrationBuilder, "IX_ExpressionEntries_Text_Trgm", "ExpressionEntries", "ExpressionText");
            CreateTrigramIndex(migrationBuilder, "IX_ExpressionEntries_Meaning_Trgm", "ExpressionEntries", "ActualMeaningText");
            CreateTrigramIndex(migrationBuilder, "IX_ExpressionEntries_Slug_Trgm", "ExpressionEntries", "Slug");
            CreateTrigramIndex(migrationBuilder, "IX_DialogueLessons_Title_Trgm", "DialogueLessons", "Title");
            CreateTrigramIndex(migrationBuilder, "IX_DialogueLessons_Description_Trgm", "DialogueLessons", "Description");
            CreateTrigramIndex(migrationBuilder, "IX_DialogueLessons_Goal_Trgm", "DialogueLessons", "LearnerGoal");
            CreateTrigramIndex(migrationBuilder, "IX_DialogueLessons_Slug_Trgm", "DialogueLessons", "Slug");
            CreateTrigramIndex(migrationBuilder, "IX_TalkTopics_Title_Trgm", "TalkTopics", "Title");
            CreateTrigramIndex(migrationBuilder, "IX_TalkTopics_Description_Trgm", "TalkTopics", "Description");
            CreateTrigramIndex(migrationBuilder, "IX_TalkTopics_Slug_Trgm", "TalkTopics", "Slug");
            CreateTrigramIndex(migrationBuilder, "IX_Exercises_Title_Trgm", "Exercises", "Title");
            CreateTrigramIndex(migrationBuilder, "IX_Exercises_Instruction_Trgm", "Exercises", "Instruction");
            CreateTrigramIndex(migrationBuilder, "IX_Exercises_Slug_Trgm", "Exercises", "Slug");
            CreateTrigramIndex(migrationBuilder, "IX_CourseLessons_Title_Trgm", "CourseLessons", "Title");
            CreateTrigramIndex(migrationBuilder, "IX_CourseLessons_Description_Trgm", "CourseLessons", "ShortDescription");
            CreateTrigramIndex(migrationBuilder, "IX_CourseLessons_Narrative_Trgm", "CourseLessons", "Narrative");
            CreateTrigramIndex(migrationBuilder, "IX_CourseLessons_Slug_Trgm", "CourseLessons", "Slug");
            CreateTrigramIndex(migrationBuilder, "IX_ExamPrepUnits_Title_Trgm", "ExamPrepUnits", "Title");
            CreateTrigramIndex(migrationBuilder, "IX_ExamPrepUnits_Description_Trgm", "ExamPrepUnits", "ShortDescription");
            CreateTrigramIndex(migrationBuilder, "IX_ExamPrepUnits_Explanation_Trgm", "ExamPrepUnits", "Explanation");
            CreateTrigramIndex(migrationBuilder, "IX_ExamPrepUnits_Slug_Trgm", "ExamPrepUnits", "Slug");
            CreateTrigramIndex(migrationBuilder, "IX_WritingTemplates_Title_Trgm", "WritingTemplates", "Title");
            CreateTrigramIndex(migrationBuilder, "IX_WritingTemplates_Description_Trgm", "WritingTemplates", "ShortDescription");
            CreateTrigramIndex(migrationBuilder, "IX_WritingTemplates_Situation_Trgm", "WritingTemplates", "Situation");
            CreateTrigramIndex(migrationBuilder, "IX_WritingTemplates_Slug_Trgm", "WritingTemplates", "Slug");
            CreateTrigramIndex(migrationBuilder, "IX_CulturalNotes_Title_Trgm", "CulturalNotes", "Title");
            CreateTrigramIndex(migrationBuilder, "IX_CulturalNotes_Description_Trgm", "CulturalNotes", "ShortDescription");
            CreateTrigramIndex(migrationBuilder, "IX_CulturalNotes_Context_Trgm", "CulturalNotes", "Context");
            CreateTrigramIndex(migrationBuilder, "IX_CulturalNotes_Slug_Trgm", "CulturalNotes", "Slug");
            CreateTrigramIndex(migrationBuilder, "IX_ConversationEvents_Name_Trgm", "ConversationEvents", "Name");
            CreateTrigramIndex(migrationBuilder, "IX_ConversationEvents_Description_Trgm", "ConversationEvents", "Description");
            CreateTrigramIndex(migrationBuilder, "IX_ConversationEvents_Organizer_Trgm", "ConversationEvents", "OrganizerName");
            CreateTrigramIndex(migrationBuilder, "IX_ConversationEvents_Slug_Trgm", "ConversationEvents", "Slug");
            CreateTrigramIndex(migrationBuilder, "IX_OrganizerProfiles_Name_Trgm", "OrganizerProfiles", "DisplayName");
            CreateTrigramIndex(migrationBuilder, "IX_OrganizerProfiles_Description_Trgm", "OrganizerProfiles", "Description");
            CreateTrigramIndex(migrationBuilder, "IX_OrganizerProfiles_Slug_Trgm", "OrganizerProfiles", "Slug");

            CreateBtreeIndex(migrationBuilder, "IX_GrammarTopics_SearchFilters", "GrammarTopics", "\"PublicationStatus\", \"CefrLevel\", \"GrammarCategory\", \"SortOrder\"");
            CreateBtreeIndex(migrationBuilder, "IX_ExpressionEntries_SearchFilters", "ExpressionEntries", "\"PublicationStatus\", \"CefrLevel\", \"ExpressionType\", \"Category\", \"SortOrder\"");
            CreateBtreeIndex(migrationBuilder, "IX_Exercises_SearchFilters", "Exercises", "\"PublicationStatus\", \"CefrLevel\", \"ExerciseType\", \"TargetSkill\", \"SortOrder\"");
            CreateBtreeIndex(migrationBuilder, "IX_CourseLessons_SearchFilters", "CourseLessons", "\"PublicationStatus\", \"CefrLevel\", \"CoursePathSlug\", \"ModuleSlug\", \"SortOrder\"");
            CreateBtreeIndex(migrationBuilder, "IX_ExamPrepUnits_SearchFilters", "ExamPrepUnits", "\"PublicationStatus\", \"CefrLevel\", \"ExamProfileKey\", \"ExamSection\", \"TaskType\", \"SortOrder\"");
            CreateBtreeIndex(migrationBuilder, "IX_WritingTemplates_SearchFilters", "WritingTemplates", "\"PublicationStatus\", \"CefrLevel\", \"Category\", \"Register\", \"SortOrder\"");
            CreateBtreeIndex(migrationBuilder, "IX_CulturalNotes_SearchFilters", "CulturalNotes", "\"PublicationStatus\", \"CefrLevel\", \"Category\", \"SortOrder\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string[] indexNames =
            [
                "IX_WordEntries_Lemma_Trgm",
                "IX_WordEntries_NormalizedLemma_Trgm",
                "IX_GrammarTopics_Title_Trgm",
                "IX_GrammarTopics_Description_Trgm",
                "IX_GrammarTopics_Slug_Trgm",
                "IX_ExpressionEntries_Text_Trgm",
                "IX_ExpressionEntries_Meaning_Trgm",
                "IX_ExpressionEntries_Slug_Trgm",
                "IX_DialogueLessons_Title_Trgm",
                "IX_DialogueLessons_Description_Trgm",
                "IX_DialogueLessons_Goal_Trgm",
                "IX_DialogueLessons_Slug_Trgm",
                "IX_TalkTopics_Title_Trgm",
                "IX_TalkTopics_Description_Trgm",
                "IX_TalkTopics_Slug_Trgm",
                "IX_Exercises_Title_Trgm",
                "IX_Exercises_Instruction_Trgm",
                "IX_Exercises_Slug_Trgm",
                "IX_CourseLessons_Title_Trgm",
                "IX_CourseLessons_Description_Trgm",
                "IX_CourseLessons_Narrative_Trgm",
                "IX_CourseLessons_Slug_Trgm",
                "IX_ExamPrepUnits_Title_Trgm",
                "IX_ExamPrepUnits_Description_Trgm",
                "IX_ExamPrepUnits_Explanation_Trgm",
                "IX_ExamPrepUnits_Slug_Trgm",
                "IX_WritingTemplates_Title_Trgm",
                "IX_WritingTemplates_Description_Trgm",
                "IX_WritingTemplates_Situation_Trgm",
                "IX_WritingTemplates_Slug_Trgm",
                "IX_CulturalNotes_Title_Trgm",
                "IX_CulturalNotes_Description_Trgm",
                "IX_CulturalNotes_Context_Trgm",
                "IX_CulturalNotes_Slug_Trgm",
                "IX_ConversationEvents_Name_Trgm",
                "IX_ConversationEvents_Description_Trgm",
                "IX_ConversationEvents_Organizer_Trgm",
                "IX_ConversationEvents_Slug_Trgm",
                "IX_OrganizerProfiles_Name_Trgm",
                "IX_OrganizerProfiles_Description_Trgm",
                "IX_OrganizerProfiles_Slug_Trgm",
                "IX_GrammarTopics_SearchFilters",
                "IX_ExpressionEntries_SearchFilters",
                "IX_Exercises_SearchFilters",
                "IX_CourseLessons_SearchFilters",
                "IX_ExamPrepUnits_SearchFilters",
                "IX_WritingTemplates_SearchFilters",
                "IX_CulturalNotes_SearchFilters",
            ];

            foreach (string indexName in indexNames)
            {
                migrationBuilder.Sql($"""DROP INDEX IF EXISTS "{indexName}";""");
            }
        }

        private static void CreateTrigramIndex(MigrationBuilder migrationBuilder, string indexName, string tableName, string columnName)
        {
            migrationBuilder.Sql($"""CREATE INDEX IF NOT EXISTS "{indexName}" ON "{tableName}" USING GIN ("{columnName}" gin_trgm_ops);""");
        }

        private static void CreateBtreeIndex(MigrationBuilder migrationBuilder, string indexName, string tableName, string columnList)
        {
            migrationBuilder.Sql($"""CREATE INDEX IF NOT EXISTS "{indexName}" ON "{tableName}" ({columnList});""");
        }
    }
}
