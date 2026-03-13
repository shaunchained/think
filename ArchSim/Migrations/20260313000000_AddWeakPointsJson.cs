using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Think.Migrations
{
    /// <inheritdoc />
    public partial class AddWeakPointsJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WeakPointsJson",
                table: "ScoreRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeakPointsJson",
                table: "ScoreRecords");
        }
    }
}
