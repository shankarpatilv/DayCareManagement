using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DayCareManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnforceCaseInsensitiveEmailAndCleanupIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teachers_Email",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Students_Email",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Immunizations_StudentId_ImmunizationId_ImmunizationDate",
                table: "Immunizations");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_Students_Email_CI\" ON \"Students\" (lower(\"Email\"));");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_Teachers_Email_CI\" ON \"Teachers\" (lower(\"Email\"));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Students_Email_CI\";");

            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Teachers_Email_CI\";");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_Email",
                table: "Teachers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_Email",
                table: "Students",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Immunizations_StudentId_ImmunizationId_ImmunizationDate",
                table: "Immunizations",
                columns: new[] { "StudentId", "ImmunizationId", "ImmunizationDate" },
                unique: true);
        }
    }
}
