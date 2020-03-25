using Microsoft.EntityFrameworkCore.Migrations;

namespace Api.Migrations
{
    public partial class RenameContentTypeToMediaType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "script_content_type",
                table: "template",
                newName: "script_media_type");

            migrationBuilder.RenameColumn(
                name: "content_type",
                table: "resource",
                newName: "media_type");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "script_media_type",
                table: "template",
                newName: "script_content_type");

            migrationBuilder.RenameColumn(
                name: "media_type",
                table: "resource",
                newName: "content_type");
        }
    }
}
