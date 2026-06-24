using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTargetLearningLanguageToUserState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserExerciseAttempts_UserId_ExerciseSlug_AttemptedAtUtc",
                table: "UserExerciseAttempts");

            migrationBuilder.DropIndex(
                name: "IX_UserContentProgress_UserId_ContentOwnerType_ContentOwnerSlug",
                table: "UserContentProgress");

            migrationBuilder.DropIndex(
                name: "IX_UserContentProgress_UserId_State",
                table: "UserContentProgress");

            migrationBuilder.DropIndex(
                name: "IX_UserContentProgress_UserId_UpdatedAtUtc",
                table: "UserContentProgress");

            migrationBuilder.AddColumn<string>(
                name: "TargetLearningLanguageCode",
                table: "UserExerciseAttempts",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "de");

            migrationBuilder.AddColumn<string>(
                name: "TargetLearningLanguageCode",
                table: "UserContentProgress",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "de");

            migrationBuilder.CreateIndex(
                name: "IX_UserExerciseAttempts_UserTargetExerciseAttempted",
                table: "UserExerciseAttempts",
                columns: new[] { "UserId", "TargetLearningLanguageCode", "ExerciseSlug", "AttemptedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_UserContentProgress_UserTargetOwner",
                table: "UserContentProgress",
                columns: new[] { "UserId", "TargetLearningLanguageCode", "ContentOwnerType", "ContentOwnerSlug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserContentProgress_UserTargetState",
                table: "UserContentProgress",
                columns: new[] { "UserId", "TargetLearningLanguageCode", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_UserContentProgress_UserTargetUpdated",
                table: "UserContentProgress",
                columns: new[] { "UserId", "TargetLearningLanguageCode", "UpdatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserExerciseAttempts_UserTargetExerciseAttempted",
                table: "UserExerciseAttempts");

            migrationBuilder.DropIndex(
                name: "IX_UserContentProgress_UserTargetOwner",
                table: "UserContentProgress");

            migrationBuilder.DropIndex(
                name: "IX_UserContentProgress_UserTargetState",
                table: "UserContentProgress");

            migrationBuilder.DropIndex(
                name: "IX_UserContentProgress_UserTargetUpdated",
                table: "UserContentProgress");

            migrationBuilder.DropColumn(
                name: "TargetLearningLanguageCode",
                table: "UserExerciseAttempts");

            migrationBuilder.DropColumn(
                name: "TargetLearningLanguageCode",
                table: "UserContentProgress");

            migrationBuilder.CreateIndex(
                name: "IX_UserExerciseAttempts_UserId_ExerciseSlug_AttemptedAtUtc",
                table: "UserExerciseAttempts",
                columns: new[] { "UserId", "ExerciseSlug", "AttemptedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_UserContentProgress_UserId_ContentOwnerType_ContentOwnerSlug",
                table: "UserContentProgress",
                columns: new[] { "UserId", "ContentOwnerType", "ContentOwnerSlug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserContentProgress_UserId_State",
                table: "UserContentProgress",
                columns: new[] { "UserId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_UserContentProgress_UserId_UpdatedAtUtc",
                table: "UserContentProgress",
                columns: new[] { "UserId", "UpdatedAtUtc" });
        }
    }
}
