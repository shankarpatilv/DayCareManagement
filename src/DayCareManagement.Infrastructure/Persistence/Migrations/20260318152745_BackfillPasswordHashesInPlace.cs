using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DayCareManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BackfillPasswordHashesInPlace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Teachers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Students",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

                        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pgcrypto;");

                        migrationBuilder.Sql("""
                                UPDATE "Students"
                                SET "Password" = encode(digest("Password", 'sha256'), 'hex')
                                WHERE "Password" IS NOT NULL
                                    AND "Password" <> ''
                                    AND "Password" !~ '^[0-9A-Fa-f]{64}$'
                                    AND left("Password", 1) <> '$';
                                """);

                        migrationBuilder.Sql("""
                                UPDATE "Teachers"
                                SET "Password" = encode(digest("Password", 'sha256'), 'hex')
                                WHERE "Password" IS NOT NULL
                                    AND "Password" <> ''
                                    AND "Password" !~ '^[0-9A-Fa-f]{64}$'
                                    AND left("Password", 1) <> '$';
                                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Teachers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Students",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);
        }
    }
}
