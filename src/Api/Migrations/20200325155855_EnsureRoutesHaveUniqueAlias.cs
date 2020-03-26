using Microsoft.EntityFrameworkCore.Migrations;

namespace Api.Migrations
{
    public partial class EnsureRoutesHaveUniqueAlias : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_zone_route_zone_id",
                table: "zone_route");

            migrationBuilder.DropIndex(
                name: "ix_terminal_route_terminal_id",
                table: "terminal_route");

            migrationBuilder.CreateIndex(
                name: "ix_zone_route_zone_id_alias",
                table: "zone_route",
                columns: new[] { "zone_id", "alias" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_terminal_route_terminal_id_alias",
                table: "terminal_route",
                columns: new[] { "terminal_id", "alias" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_zone_route_zone_id_alias",
                table: "zone_route");

            migrationBuilder.DropIndex(
                name: "ix_terminal_route_terminal_id_alias",
                table: "terminal_route");

            migrationBuilder.CreateIndex(
                name: "ix_zone_route_zone_id",
                table: "zone_route",
                column: "zone_id");

            migrationBuilder.CreateIndex(
                name: "ix_terminal_route_terminal_id",
                table: "terminal_route",
                column: "terminal_id");
        }
    }
}
