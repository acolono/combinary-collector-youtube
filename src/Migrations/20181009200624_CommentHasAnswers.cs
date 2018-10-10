using Microsoft.EntityFrameworkCore.Migrations;

namespace YoutubeCollector.Migrations
{
    public partial class CommentHasAnswers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasAnswers",
                table: "Comments",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_HasAnswers",
                table: "Comments",
                column: "HasAnswers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comments_HasAnswers",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "HasAnswers",
                table: "Comments");
        }
    }
}
