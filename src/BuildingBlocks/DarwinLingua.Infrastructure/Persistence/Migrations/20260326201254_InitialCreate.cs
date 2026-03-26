using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PackageId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    PackageVersion = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    PackageName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    SourceType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    InputFileName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    TotalEntries = table.Column<int>(type: "INTEGER", nullable: false),
                    InsertedEntries = table.Column<int>(type: "INTEGER", nullable: false),
                    SkippedDuplicateEntries = table.Column<int>(type: "INTEGER", nullable: false),
                    InvalidEntries = table.Column<int>(type: "INTEGER", nullable: false),
                    WarningCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentPackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    EnglishName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    NativeName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    SupportsUserInterface = table.Column<bool>(type: "INTEGER", nullable: false),
                    SupportsMeanings = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsSystem = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserFavoriteWords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    WordEntryPublicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavoriteWords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserLearningProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    PreferredMeaningLanguage1 = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    PreferredMeaningLanguage2 = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    UiLanguageCode = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLearningProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserWordStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    WordEntryPublicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsKnown = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDifficult = table.Column<bool>(type: "INTEGER", nullable: false),
                    FirstViewedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastViewedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ViewCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWordStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WordEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PublicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Lemma = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    NormalizedLemma = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    PrimaryCefrLevel = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    PartOfSpeech = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Article = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    PluralForm = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    InfinitiveForm = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    PronunciationIpa = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    SyllableBreak = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    PublicationStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ContentSourceType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SourceReference = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentPackageEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContentPackageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RawLemma = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    NormalizedLemma = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    CefrLevel = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    PartOfSpeech = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    ProcessingStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    WarningMessage = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    ImportedWordEntryPublicId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentPackageEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentPackageEntries_ContentPackages_ContentPackageId",
                        column: x => x.ContentPackageId,
                        principalTable: "ContentPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TopicLocalizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicLocalizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TopicLocalizations_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WordSenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WordEntryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SenseOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPrimarySense = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShortDefinitionDe = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    ShortGloss = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    PublicationStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordSenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WordSenses_WordEntries_WordEntryId",
                        column: x => x.WordEntryId,
                        principalTable: "WordEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WordTopics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WordEntryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsPrimaryTopic = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WordTopics_WordEntries_WordEntryId",
                        column: x => x.WordEntryId,
                        principalTable: "WordEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExampleSentences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WordSenseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SentenceOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    GermanText = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    IsPrimaryExample = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExampleSentences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExampleSentences_WordSenses_WordSenseId",
                        column: x => x.WordSenseId,
                        principalTable: "WordSenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SenseTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WordSenseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    TranslationText = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SenseTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SenseTranslations_WordSenses_WordSenseId",
                        column: x => x.WordSenseId,
                        principalTable: "WordSenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExampleTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExampleSentenceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    TranslationText = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExampleTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExampleTranslations_ExampleSentences_ExampleSentenceId",
                        column: x => x.ExampleSentenceId,
                        principalTable: "ExampleSentences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentPackageEntries_ContentPackageId",
                table: "ContentPackageEntries",
                column: "ContentPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentPackages_PackageId",
                table: "ContentPackages",
                column: "PackageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExampleSentences_WordSenseId_SentenceOrder",
                table: "ExampleSentences",
                columns: new[] { "WordSenseId", "SentenceOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExampleTranslations_ExampleSentenceId_LanguageCode",
                table: "ExampleTranslations",
                columns: new[] { "ExampleSentenceId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Languages_Code",
                table: "Languages",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SenseTranslations_WordSenseId_LanguageCode",
                table: "SenseTranslations",
                columns: new[] { "WordSenseId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TopicLocalizations_TopicId_LanguageCode",
                table: "TopicLocalizations",
                columns: new[] { "TopicId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Topics_Key",
                table: "Topics",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteWords_UserId_WordEntryPublicId",
                table: "UserFavoriteWords",
                columns: new[] { "UserId", "WordEntryPublicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLearningProfiles_UserId",
                table: "UserLearningProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserWordStates_UserId_WordEntryPublicId",
                table: "UserWordStates",
                columns: new[] { "UserId", "WordEntryPublicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WordEntries_Browse_Cefr_NormalizedLemma",
                table: "WordEntries",
                columns: new[] { "PrimaryCefrLevel", "NormalizedLemma" });

            migrationBuilder.CreateIndex(
                name: "IX_WordEntries_NormalizedLemma_PartOfSpeech_PrimaryCefrLevel",
                table: "WordEntries",
                columns: new[] { "NormalizedLemma", "PartOfSpeech", "PrimaryCefrLevel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WordEntries_PublicId",
                table: "WordEntries",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WordEntries_Search_ActiveNormalizedLemma",
                table: "WordEntries",
                columns: new[] { "PublicationStatus", "NormalizedLemma" });

            migrationBuilder.CreateIndex(
                name: "IX_WordEntries_Search_NormalizedLemma",
                table: "WordEntries",
                column: "NormalizedLemma");

            migrationBuilder.CreateIndex(
                name: "IX_WordSenses_WordEntryId_SenseOrder",
                table: "WordSenses",
                columns: new[] { "WordEntryId", "SenseOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WordTopics_WordEntryId_TopicId",
                table: "WordTopics",
                columns: new[] { "WordEntryId", "TopicId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentPackageEntries");

            migrationBuilder.DropTable(
                name: "ExampleTranslations");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "SenseTranslations");

            migrationBuilder.DropTable(
                name: "TopicLocalizations");

            migrationBuilder.DropTable(
                name: "UserFavoriteWords");

            migrationBuilder.DropTable(
                name: "UserLearningProfiles");

            migrationBuilder.DropTable(
                name: "UserWordStates");

            migrationBuilder.DropTable(
                name: "WordTopics");

            migrationBuilder.DropTable(
                name: "ContentPackages");

            migrationBuilder.DropTable(
                name: "ExampleSentences");

            migrationBuilder.DropTable(
                name: "Topics");

            migrationBuilder.DropTable(
                name: "WordSenses");

            migrationBuilder.DropTable(
                name: "WordEntries");
        }
    }
}
