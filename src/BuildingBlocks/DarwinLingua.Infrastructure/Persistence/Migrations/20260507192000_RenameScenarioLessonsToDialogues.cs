using DarwinLingua.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DarwinLinguaDbContext))]
    [Migration("20260507192000_RenameScenarioLessonsToDialogues")]
    public partial class RenameScenarioLessonsToDialogues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(name: "ScenarioLessons", newName: "DialogueLessons");
            migrationBuilder.RenameTable(name: "ScenarioLessonTopics", newName: "DialogueLessonTopics");
            migrationBuilder.RenameTable(name: "ScenarioDialogueTurns", newName: "DialogueTurns");
            migrationBuilder.RenameTable(name: "ScenarioPhrases", newName: "DialoguePhrases");
            migrationBuilder.RenameTable(name: "ScenarioQuestions", newName: "DialogueQuestions");
            migrationBuilder.RenameTable(name: "ScenarioAnswers", newName: "DialogueAnswers");
            migrationBuilder.RenameTable(name: "ScenarioDialogueTurnTranslations", newName: "DialogueTurnTranslations");
            migrationBuilder.RenameTable(name: "ScenarioPhraseTranslations", newName: "DialoguePhraseTranslations");
            migrationBuilder.RenameTable(name: "ScenarioQuestionTranslations", newName: "DialogueQuestionTranslations");
            migrationBuilder.RenameTable(name: "ScenarioAnswerTranslations", newName: "DialogueAnswerTranslations");
            migrationBuilder.RenameTable(name: "ConversationStarterLinkedScenarios", newName: "ConversationStarterLinkedDialogues");
            migrationBuilder.RenameTable(name: "EventPreparationLinkedScenarios", newName: "EventPreparationLinkedDialogues");

            migrationBuilder.RenameColumn(name: "ScenarioLessonId", table: "DialogueLessonTopics", newName: "DialogueLessonId");
            migrationBuilder.RenameColumn(name: "ScenarioLessonId", table: "DialogueTurns", newName: "DialogueLessonId");
            migrationBuilder.RenameColumn(name: "ScenarioLessonId", table: "DialoguePhrases", newName: "DialogueLessonId");
            migrationBuilder.RenameColumn(name: "ScenarioLessonId", table: "DialogueQuestions", newName: "DialogueLessonId");
            migrationBuilder.RenameColumn(name: "ScenarioQuestionId", table: "DialogueAnswers", newName: "DialogueQuestionId");
            migrationBuilder.RenameColumn(name: "ScenarioSlug", table: "ConversationStarterLinkedDialogues", newName: "DialogueSlug");
            migrationBuilder.RenameColumn(name: "ScenarioSlug", table: "EventPreparationLinkedDialogues", newName: "DialogueSlug");

            if (ActiveProvider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                RecreateDialogueIndexes(migrationBuilder);
            }
            else
            {
                migrationBuilder.RenameIndex(name: "IX_ScenarioLessons_Slug", table: "DialogueLessons", newName: "IX_DialogueLessons_Slug");
                migrationBuilder.RenameIndex(name: "IX_ScenarioLessonTopics_PrimaryPerLesson", table: "DialogueLessonTopics", newName: "IX_DialogueLessonTopics_PrimaryPerLesson");
                migrationBuilder.RenameIndex(name: "IX_ScenarioLessonTopics_ScenarioLessonId_TopicId", table: "DialogueLessonTopics", newName: "IX_DialogueLessonTopics_DialogueLessonId_TopicId");
                migrationBuilder.RenameIndex(name: "IX_ScenarioLessonTopics_TopicId", table: "DialogueLessonTopics", newName: "IX_DialogueLessonTopics_TopicId");
                migrationBuilder.RenameIndex(name: "IX_ScenarioDialogueTurns_ScenarioLessonId_SortOrder", table: "DialogueTurns", newName: "IX_DialogueTurns_DialogueLessonId_SortOrder");
                migrationBuilder.RenameIndex(name: "IX_ScenarioPhrases_ScenarioLessonId_SortOrder", table: "DialoguePhrases", newName: "IX_DialoguePhrases_DialogueLessonId_SortOrder");
                migrationBuilder.RenameIndex(name: "IX_ScenarioQuestions_ScenarioLessonId_SortOrder", table: "DialogueQuestions", newName: "IX_DialogueQuestions_DialogueLessonId_SortOrder");
                migrationBuilder.RenameIndex(name: "IX_ScenarioAnswers_ScenarioQuestionId_SortOrder", table: "DialogueAnswers", newName: "IX_DialogueAnswers_DialogueQuestionId_SortOrder");
                migrationBuilder.RenameIndex(name: "IX_ScenarioDialogueTurnTranslations_OwnerId_LanguageCode", table: "DialogueTurnTranslations", newName: "IX_DialogueTurnTranslations_OwnerId_LanguageCode");
                migrationBuilder.RenameIndex(name: "IX_ScenarioPhraseTranslations_OwnerId_LanguageCode", table: "DialoguePhraseTranslations", newName: "IX_DialoguePhraseTranslations_OwnerId_LanguageCode");
                migrationBuilder.RenameIndex(name: "IX_ScenarioQuestionTranslations_OwnerId_LanguageCode", table: "DialogueQuestionTranslations", newName: "IX_DialogueQuestionTranslations_OwnerId_LanguageCode");
                migrationBuilder.RenameIndex(name: "IX_ScenarioAnswerTranslations_OwnerId_LanguageCode", table: "DialogueAnswerTranslations", newName: "IX_DialogueAnswerTranslations_OwnerId_LanguageCode");
                migrationBuilder.RenameIndex(name: "IX_ConversationStarterLinkedScenarios_ConversationStarterPackId_ScenarioSlug", table: "ConversationStarterLinkedDialogues", newName: "IX_ConversationStarterLinkedDialogues_ConversationStarterPackId_DialogueSlug");
                migrationBuilder.RenameIndex(name: "IX_EventPreparationLinkedScenarios_EventPreparationPackId_ScenarioSlug", table: "EventPreparationLinkedDialogues", newName: "IX_EventPreparationLinkedDialogues_EventPreparationPackId_DialogueSlug");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                RecreateScenarioIndexes(migrationBuilder);
            }
            else
            {
                migrationBuilder.RenameIndex(name: "IX_DialogueAnswerTranslations_OwnerId_LanguageCode", table: "DialogueAnswerTranslations", newName: "IX_ScenarioAnswerTranslations_OwnerId_LanguageCode");
                migrationBuilder.RenameIndex(name: "IX_DialogueQuestionTranslations_OwnerId_LanguageCode", table: "DialogueQuestionTranslations", newName: "IX_ScenarioQuestionTranslations_OwnerId_LanguageCode");
                migrationBuilder.RenameIndex(name: "IX_DialoguePhraseTranslations_OwnerId_LanguageCode", table: "DialoguePhraseTranslations", newName: "IX_ScenarioPhraseTranslations_OwnerId_LanguageCode");
                migrationBuilder.RenameIndex(name: "IX_DialogueTurnTranslations_OwnerId_LanguageCode", table: "DialogueTurnTranslations", newName: "IX_ScenarioDialogueTurnTranslations_OwnerId_LanguageCode");
                migrationBuilder.RenameIndex(name: "IX_DialogueAnswers_DialogueQuestionId_SortOrder", table: "DialogueAnswers", newName: "IX_ScenarioAnswers_ScenarioQuestionId_SortOrder");
                migrationBuilder.RenameIndex(name: "IX_DialogueQuestions_DialogueLessonId_SortOrder", table: "DialogueQuestions", newName: "IX_ScenarioQuestions_ScenarioLessonId_SortOrder");
                migrationBuilder.RenameIndex(name: "IX_DialoguePhrases_DialogueLessonId_SortOrder", table: "DialoguePhrases", newName: "IX_ScenarioPhrases_ScenarioLessonId_SortOrder");
                migrationBuilder.RenameIndex(name: "IX_DialogueTurns_DialogueLessonId_SortOrder", table: "DialogueTurns", newName: "IX_ScenarioDialogueTurns_ScenarioLessonId_SortOrder");
                migrationBuilder.RenameIndex(name: "IX_DialogueLessonTopics_TopicId", table: "DialogueLessonTopics", newName: "IX_ScenarioLessonTopics_TopicId");
                migrationBuilder.RenameIndex(name: "IX_DialogueLessonTopics_DialogueLessonId_TopicId", table: "DialogueLessonTopics", newName: "IX_ScenarioLessonTopics_ScenarioLessonId_TopicId");
                migrationBuilder.RenameIndex(name: "IX_DialogueLessonTopics_PrimaryPerLesson", table: "DialogueLessonTopics", newName: "IX_ScenarioLessonTopics_PrimaryPerLesson");
                migrationBuilder.RenameIndex(name: "IX_DialogueLessons_Slug", table: "DialogueLessons", newName: "IX_ScenarioLessons_Slug");
                migrationBuilder.RenameIndex(name: "IX_ConversationStarterLinkedDialogues_ConversationStarterPackId_DialogueSlug", table: "ConversationStarterLinkedDialogues", newName: "IX_ConversationStarterLinkedScenarios_ConversationStarterPackId_ScenarioSlug");
                migrationBuilder.RenameIndex(name: "IX_EventPreparationLinkedDialogues_EventPreparationPackId_DialogueSlug", table: "EventPreparationLinkedDialogues", newName: "IX_EventPreparationLinkedScenarios_EventPreparationPackId_ScenarioSlug");
            }

            migrationBuilder.RenameColumn(name: "DialogueSlug", table: "EventPreparationLinkedDialogues", newName: "ScenarioSlug");
            migrationBuilder.RenameColumn(name: "DialogueSlug", table: "ConversationStarterLinkedDialogues", newName: "ScenarioSlug");
            migrationBuilder.RenameColumn(name: "DialogueQuestionId", table: "DialogueAnswers", newName: "ScenarioQuestionId");
            migrationBuilder.RenameColumn(name: "DialogueLessonId", table: "DialogueQuestions", newName: "ScenarioLessonId");
            migrationBuilder.RenameColumn(name: "DialogueLessonId", table: "DialoguePhrases", newName: "ScenarioLessonId");
            migrationBuilder.RenameColumn(name: "DialogueLessonId", table: "DialogueTurns", newName: "ScenarioLessonId");
            migrationBuilder.RenameColumn(name: "DialogueLessonId", table: "DialogueLessonTopics", newName: "ScenarioLessonId");

            migrationBuilder.RenameTable(name: "EventPreparationLinkedDialogues", newName: "EventPreparationLinkedScenarios");
            migrationBuilder.RenameTable(name: "ConversationStarterLinkedDialogues", newName: "ConversationStarterLinkedScenarios");
            migrationBuilder.RenameTable(name: "DialogueAnswerTranslations", newName: "ScenarioAnswerTranslations");
            migrationBuilder.RenameTable(name: "DialogueQuestionTranslations", newName: "ScenarioQuestionTranslations");
            migrationBuilder.RenameTable(name: "DialoguePhraseTranslations", newName: "ScenarioPhraseTranslations");
            migrationBuilder.RenameTable(name: "DialogueTurnTranslations", newName: "ScenarioDialogueTurnTranslations");
            migrationBuilder.RenameTable(name: "DialogueAnswers", newName: "ScenarioAnswers");
            migrationBuilder.RenameTable(name: "DialogueQuestions", newName: "ScenarioQuestions");
            migrationBuilder.RenameTable(name: "DialoguePhrases", newName: "ScenarioPhrases");
            migrationBuilder.RenameTable(name: "DialogueTurns", newName: "ScenarioDialogueTurns");
            migrationBuilder.RenameTable(name: "DialogueLessonTopics", newName: "ScenarioLessonTopics");
            migrationBuilder.RenameTable(name: "DialogueLessons", newName: "ScenarioLessons");
        }

        private static void RecreateDialogueIndexes(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_ScenarioLessons_Slug", table: "DialogueLessons");
            migrationBuilder.DropIndex(name: "IX_ScenarioLessonTopics_PrimaryPerLesson", table: "DialogueLessonTopics");
            migrationBuilder.DropIndex(name: "IX_ScenarioLessonTopics_ScenarioLessonId_TopicId", table: "DialogueLessonTopics");
            migrationBuilder.DropIndex(name: "IX_ScenarioLessonTopics_TopicId", table: "DialogueLessonTopics");
            migrationBuilder.DropIndex(name: "IX_ScenarioDialogueTurns_ScenarioLessonId_SortOrder", table: "DialogueTurns");
            migrationBuilder.DropIndex(name: "IX_ScenarioPhrases_ScenarioLessonId_SortOrder", table: "DialoguePhrases");
            migrationBuilder.DropIndex(name: "IX_ScenarioQuestions_ScenarioLessonId_SortOrder", table: "DialogueQuestions");
            migrationBuilder.DropIndex(name: "IX_ScenarioAnswers_ScenarioQuestionId_SortOrder", table: "DialogueAnswers");
            migrationBuilder.DropIndex(name: "IX_ScenarioDialogueTurnTranslations_OwnerId_LanguageCode", table: "DialogueTurnTranslations");
            migrationBuilder.DropIndex(name: "IX_ScenarioPhraseTranslations_OwnerId_LanguageCode", table: "DialoguePhraseTranslations");
            migrationBuilder.DropIndex(name: "IX_ScenarioQuestionTranslations_OwnerId_LanguageCode", table: "DialogueQuestionTranslations");
            migrationBuilder.DropIndex(name: "IX_ScenarioAnswerTranslations_OwnerId_LanguageCode", table: "DialogueAnswerTranslations");
            migrationBuilder.DropIndex(name: "IX_ConversationStarterLinkedScenarios_ConversationStarterPackId_ScenarioSlug", table: "ConversationStarterLinkedDialogues");
            migrationBuilder.DropIndex(name: "IX_EventPreparationLinkedScenarios_EventPreparationPackId_ScenarioSlug", table: "EventPreparationLinkedDialogues");

            CreateDialogueIndexes(migrationBuilder);
        }

        private static void RecreateScenarioIndexes(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_DialogueAnswerTranslations_OwnerId_LanguageCode", table: "DialogueAnswerTranslations");
            migrationBuilder.DropIndex(name: "IX_DialogueQuestionTranslations_OwnerId_LanguageCode", table: "DialogueQuestionTranslations");
            migrationBuilder.DropIndex(name: "IX_DialoguePhraseTranslations_OwnerId_LanguageCode", table: "DialoguePhraseTranslations");
            migrationBuilder.DropIndex(name: "IX_DialogueTurnTranslations_OwnerId_LanguageCode", table: "DialogueTurnTranslations");
            migrationBuilder.DropIndex(name: "IX_DialogueAnswers_DialogueQuestionId_SortOrder", table: "DialogueAnswers");
            migrationBuilder.DropIndex(name: "IX_DialogueQuestions_DialogueLessonId_SortOrder", table: "DialogueQuestions");
            migrationBuilder.DropIndex(name: "IX_DialoguePhrases_DialogueLessonId_SortOrder", table: "DialoguePhrases");
            migrationBuilder.DropIndex(name: "IX_DialogueTurns_DialogueLessonId_SortOrder", table: "DialogueTurns");
            migrationBuilder.DropIndex(name: "IX_DialogueLessonTopics_TopicId", table: "DialogueLessonTopics");
            migrationBuilder.DropIndex(name: "IX_DialogueLessonTopics_DialogueLessonId_TopicId", table: "DialogueLessonTopics");
            migrationBuilder.DropIndex(name: "IX_DialogueLessonTopics_PrimaryPerLesson", table: "DialogueLessonTopics");
            migrationBuilder.DropIndex(name: "IX_DialogueLessons_Slug", table: "DialogueLessons");
            migrationBuilder.DropIndex(name: "IX_ConversationStarterLinkedDialogues_ConversationStarterPackId_DialogueSlug", table: "ConversationStarterLinkedDialogues");
            migrationBuilder.DropIndex(name: "IX_EventPreparationLinkedDialogues_EventPreparationPackId_DialogueSlug", table: "EventPreparationLinkedDialogues");

            CreateScenarioIndexes(migrationBuilder);
        }

        private static void CreateDialogueIndexes(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(name: "IX_DialogueLessons_Slug", table: "DialogueLessons", column: "Slug", unique: true);
            migrationBuilder.CreateIndex(name: "IX_DialogueLessonTopics_PrimaryPerLesson", table: "DialogueLessonTopics", column: "DialogueLessonId", unique: true, filter: "\"IsPrimary\"");
            migrationBuilder.CreateIndex(name: "IX_DialogueLessonTopics_DialogueLessonId_TopicId", table: "DialogueLessonTopics", columns: new[] { "DialogueLessonId", "TopicId" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_DialogueLessonTopics_TopicId", table: "DialogueLessonTopics", column: "TopicId");
            migrationBuilder.CreateIndex(name: "IX_DialogueTurns_DialogueLessonId_SortOrder", table: "DialogueTurns", columns: new[] { "DialogueLessonId", "SortOrder" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_DialoguePhrases_DialogueLessonId_SortOrder", table: "DialoguePhrases", columns: new[] { "DialogueLessonId", "SortOrder" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_DialogueQuestions_DialogueLessonId_SortOrder", table: "DialogueQuestions", columns: new[] { "DialogueLessonId", "SortOrder" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_DialogueAnswers_DialogueQuestionId_SortOrder", table: "DialogueAnswers", columns: new[] { "DialogueQuestionId", "SortOrder" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_DialogueTurnTranslations_OwnerId_LanguageCode", table: "DialogueTurnTranslations", columns: new[] { "OwnerId", "LanguageCode" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_DialoguePhraseTranslations_OwnerId_LanguageCode", table: "DialoguePhraseTranslations", columns: new[] { "OwnerId", "LanguageCode" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_DialogueQuestionTranslations_OwnerId_LanguageCode", table: "DialogueQuestionTranslations", columns: new[] { "OwnerId", "LanguageCode" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_DialogueAnswerTranslations_OwnerId_LanguageCode", table: "DialogueAnswerTranslations", columns: new[] { "OwnerId", "LanguageCode" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_ConversationStarterLinkedDialogues_ConversationStarterPackId_DialogueSlug", table: "ConversationStarterLinkedDialogues", columns: new[] { "ConversationStarterPackId", "DialogueSlug" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_EventPreparationLinkedDialogues_EventPreparationPackId_DialogueSlug", table: "EventPreparationLinkedDialogues", columns: new[] { "EventPreparationPackId", "DialogueSlug" }, unique: true);
        }

        private static void CreateScenarioIndexes(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(name: "IX_ScenarioLessons_Slug", table: "DialogueLessons", column: "Slug", unique: true);
            migrationBuilder.CreateIndex(name: "IX_ScenarioLessonTopics_PrimaryPerLesson", table: "DialogueLessonTopics", column: "DialogueLessonId", unique: true, filter: "\"IsPrimary\"");
            migrationBuilder.CreateIndex(name: "IX_ScenarioLessonTopics_ScenarioLessonId_TopicId", table: "DialogueLessonTopics", columns: new[] { "DialogueLessonId", "TopicId" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_ScenarioLessonTopics_TopicId", table: "DialogueLessonTopics", column: "TopicId");
            migrationBuilder.CreateIndex(name: "IX_ScenarioDialogueTurns_ScenarioLessonId_SortOrder", table: "DialogueTurns", columns: new[] { "DialogueLessonId", "SortOrder" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_ScenarioPhrases_ScenarioLessonId_SortOrder", table: "DialoguePhrases", columns: new[] { "DialogueLessonId", "SortOrder" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_ScenarioQuestions_ScenarioLessonId_SortOrder", table: "DialogueQuestions", columns: new[] { "DialogueLessonId", "SortOrder" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_ScenarioAnswers_ScenarioQuestionId_SortOrder", table: "DialogueAnswers", columns: new[] { "DialogueQuestionId", "SortOrder" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_ScenarioDialogueTurnTranslations_OwnerId_LanguageCode", table: "DialogueTurnTranslations", columns: new[] { "OwnerId", "LanguageCode" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_ScenarioPhraseTranslations_OwnerId_LanguageCode", table: "DialoguePhraseTranslations", columns: new[] { "OwnerId", "LanguageCode" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_ScenarioQuestionTranslations_OwnerId_LanguageCode", table: "DialogueQuestionTranslations", columns: new[] { "OwnerId", "LanguageCode" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_ScenarioAnswerTranslations_OwnerId_LanguageCode", table: "DialogueAnswerTranslations", columns: new[] { "OwnerId", "LanguageCode" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_ConversationStarterLinkedScenarios_ConversationStarterPackId_ScenarioSlug", table: "ConversationStarterLinkedDialogues", columns: new[] { "ConversationStarterPackId", "DialogueSlug" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_EventPreparationLinkedScenarios_EventPreparationPackId_ScenarioSlug", table: "EventPreparationLinkedDialogues", columns: new[] { "EventPreparationPackId", "DialogueSlug" }, unique: true);
        }
    }
}
