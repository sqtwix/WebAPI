using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CsvImportApi.Migrations
{
    /// <inheritdoc />
    public partial class Values_Records : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "minValue",
                table: "Results",
                newName: "MinValue");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Results",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Results");

            migrationBuilder.RenameColumn(
                name: "MinValue",
                table: "Results",
                newName: "minValue");
        }
    }
}
