using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rtfx.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddLastModifiedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifierDate",
                table: "Artifacts",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "ArtifactMetdata",
                type: "TEXT",
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1024);

            migrationBuilder.CreateTable(
                name: "ArtifactSourceVersion",
                columns: table => new
                {
                    ArtifactSourceVersionId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ArtifactId = table.Column<long>(type: "INTEGER", nullable: false),
                    Branch = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    Commit = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtifactSourceVersion", x => x.ArtifactSourceVersionId);
                    table.ForeignKey(
                        name: "FK_ArtifactSourceVersion_Artifacts_ArtifactId",
                        column: x => x.ArtifactId,
                        principalTable: "Artifacts",
                        principalColumn: "ArtifactId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactSourceVersion_ArtifactId",
                table: "ArtifactSourceVersion",
                column: "ArtifactId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtifactSourceVersion");

            migrationBuilder.DropColumn(
                name: "LastModifierDate",
                table: "Artifacts");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "ArtifactMetdata",
                type: "TEXT",
                maxLength: 1024,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1024,
                oldNullable: true);
        }
    }
}
