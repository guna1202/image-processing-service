using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImageProcessing.Migrations
{
    /// <inheritdoc />
    public partial class AddThumbnailFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThumbnailFileName",
                table: "Images",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailStorageKey",
                table: "Images",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "Images",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThumbnailFileName",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "ThumbnailStorageKey",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "Images");
        }
    }
}
