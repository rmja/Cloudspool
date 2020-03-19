using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Api.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "documents",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<int>(nullable: false),
                    template_id = table.Column<int>(nullable: true),
                    content = table.Column<byte[]>(nullable: true),
                    content_type = table.Column<string>(maxLength: 100, nullable: false),
                    created = table.Column<DateTime>(nullable: false),
                    deleted = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_documents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(maxLength: 100, nullable: false),
                    key = table.Column<Guid>(nullable: false),
                    created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_projects", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "resources",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<int>(nullable: false),
                    alias = table.Column<string>(maxLength: 100, nullable: false),
                    content_type = table.Column<string>(maxLength: 100, nullable: false),
                    content = table.Column<byte[]>(nullable: true),
                    created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resources", x => x.id);
                    table.ForeignKey(
                        name: "fk_resources_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "templates",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<int>(nullable: false),
                    name = table.Column<string>(maxLength: 100, nullable: false),
                    script = table.Column<string>(nullable: false),
                    script_content_type = table.Column<string>(maxLength: 100, nullable: false),
                    created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_templates", x => x.id);
                    table.ForeignKey(
                        name: "fk_templates_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "zones",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<int>(nullable: false),
                    name = table.Column<string>(maxLength: 100, nullable: false),
                    created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_zones", x => x.id);
                    table.ForeignKey(
                        name: "fk_zones_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "formats",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    zone_id = table.Column<int>(nullable: false),
                    alias = table.Column<string>(maxLength: 100, nullable: false),
                    template_id = table.Column<int>(nullable: false),
                    created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_formats", x => x.id);
                    table.ForeignKey(
                        name: "fk_formats_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_formats_zones_zone_id",
                        column: x => x.zone_id,
                        principalTable: "zones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "spoolers",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    zone_id = table.Column<int>(nullable: false),
                    name = table.Column<string>(maxLength: 100, nullable: false),
                    key = table.Column<Guid>(nullable: false),
                    created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_spoolers", x => x.id);
                    table.ForeignKey(
                        name: "fk_spoolers_zones_zone_id",
                        column: x => x.zone_id,
                        principalTable: "zones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "terminals",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    zone_id = table.Column<int>(nullable: false),
                    name = table.Column<string>(maxLength: 100, nullable: false),
                    key = table.Column<Guid>(nullable: false),
                    created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_terminals", x => x.id);
                    table.ForeignKey(
                        name: "fk_terminals_zones_zone_id",
                        column: x => x.zone_id,
                        principalTable: "zones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "zone_route",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    zone_id = table.Column<int>(nullable: false),
                    index = table.Column<int>(nullable: false),
                    alias = table.Column<string>(maxLength: 100, nullable: false),
                    spooler_id = table.Column<int>(nullable: false),
                    printer_name = table.Column<string>(nullable: true),
                    created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_zone_route", x => x.id);
                    table.ForeignKey(
                        name: "fk_zone_route_spoolers_spooler_id",
                        column: x => x.spooler_id,
                        principalTable: "spoolers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_zone_route_zones_zone_id",
                        column: x => x.zone_id,
                        principalTable: "zones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "terminal_route",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    terminal_id = table.Column<int>(nullable: false),
                    index = table.Column<int>(nullable: false),
                    alias = table.Column<string>(maxLength: 100, nullable: false),
                    spooler_id = table.Column<int>(nullable: false),
                    printer_name = table.Column<string>(nullable: true),
                    created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_terminal_route", x => x.id);
                    table.ForeignKey(
                        name: "fk_terminal_route_spoolers_spooler_id",
                        column: x => x.spooler_id,
                        principalTable: "spoolers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_terminal_route_terminals_terminal_id",
                        column: x => x.terminal_id,
                        principalTable: "terminals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_formats_template_id",
                table: "formats",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "ix_formats_zone_id_alias",
                table: "formats",
                columns: new[] { "zone_id", "alias" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_projects_key",
                table: "projects",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_resources_project_id_alias",
                table: "resources",
                columns: new[] { "project_id", "alias" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_spoolers_key",
                table: "spoolers",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_spoolers_zone_id",
                table: "spoolers",
                column: "zone_id");

            migrationBuilder.CreateIndex(
                name: "ix_templates_project_id",
                table: "templates",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_terminal_route_spooler_id",
                table: "terminal_route",
                column: "spooler_id");

            migrationBuilder.CreateIndex(
                name: "ix_terminal_route_terminal_id",
                table: "terminal_route",
                column: "terminal_id");

            migrationBuilder.CreateIndex(
                name: "ix_terminals_key",
                table: "terminals",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_terminals_zone_id",
                table: "terminals",
                column: "zone_id");

            migrationBuilder.CreateIndex(
                name: "ix_zone_route_spooler_id",
                table: "zone_route",
                column: "spooler_id");

            migrationBuilder.CreateIndex(
                name: "ix_zone_route_zone_id",
                table: "zone_route",
                column: "zone_id");

            migrationBuilder.CreateIndex(
                name: "ix_zones_project_id",
                table: "zones",
                column: "project_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "documents");

            migrationBuilder.DropTable(
                name: "formats");

            migrationBuilder.DropTable(
                name: "resources");

            migrationBuilder.DropTable(
                name: "terminal_route");

            migrationBuilder.DropTable(
                name: "zone_route");

            migrationBuilder.DropTable(
                name: "templates");

            migrationBuilder.DropTable(
                name: "terminals");

            migrationBuilder.DropTable(
                name: "spoolers");

            migrationBuilder.DropTable(
                name: "zones");

            migrationBuilder.DropTable(
                name: "projects");
        }
    }
}
