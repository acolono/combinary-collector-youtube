using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace YoutubeCollector.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Videos",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    PublishedAt = table.Column<DateTime>(nullable: true),
                    ChannelId = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Thumbnail = table.Column<string>(nullable: true),
                    MaxResImage = table.Column<string>(nullable: true),
                    ChannelTitle = table.Column<string>(nullable: true),
                    Tags = table.Column<string[]>(nullable: true),
                    Duration = table.Column<string>(nullable: true),
                    CaptionsAvailable = table.Column<bool>(nullable: false),
                    Rating = table.Column<string>(nullable: true),
                    Embeddable = table.Column<bool>(nullable: false),
                    TopicCategories = table.Column<string[]>(nullable: true),
                    HasComments = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    VideoId = table.Column<string>(nullable: true),
                    AuthorDisplayName = table.Column<string>(nullable: true),
                    AuthorProfileImageUrl = table.Column<string>(nullable: true),
                    AuthorChannelUrl = table.Column<string>(nullable: true),
                    OriginalText = table.Column<string>(nullable: true),
                    ParentId = table.Column<string>(nullable: true),
                    LikeCount = table.Column<long>(nullable: true),
                    ModerationStatus = table.Column<string>(nullable: true),
                    PublishedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    CommentType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Statistics",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    CommentCount = table.Column<decimal>(nullable: true),
                    DislikeCount = table.Column<decimal>(nullable: true),
                    LikeCount = table.Column<decimal>(nullable: true),
                    ViewCount = table.Column<decimal>(nullable: true),
                    VideoId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Statistics_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_CommentType",
                table: "Comments",
                column: "CommentType");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_VideoId",
                table: "Comments",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_Statistics_VideoId",
                table: "Statistics",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_HasComments",
                table: "Videos",
                column: "HasComments");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Statistics");

            migrationBuilder.DropTable(
                name: "Videos");
        }
    }
}
