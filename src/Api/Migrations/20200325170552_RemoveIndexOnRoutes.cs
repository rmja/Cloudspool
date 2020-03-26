using Microsoft.EntityFrameworkCore.Migrations;

namespace Api.Migrations
{
    public partial class RemoveIndexOnRoutes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "index",
                table: "zone_route");

            migrationBuilder.DropColumn(
                name: "index",
                table: "terminal_route");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "index",
                table: "zone_route",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "index",
                table: "terminal_route",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
