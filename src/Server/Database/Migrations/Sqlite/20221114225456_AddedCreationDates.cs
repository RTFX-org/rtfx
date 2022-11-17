using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Rtfx.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddedCreationDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "Packages",
                type: "TEXT",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "Feeds",
                type: "TEXT",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "Artifacts",
                type: "TEXT",
                nullable: false,
                defaultValue: DateTime.UtcNow);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "Feeds");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "Artifacts");
        }
    }
}
