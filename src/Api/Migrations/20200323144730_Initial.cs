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
                name: "document",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<int>(nullable: false),
                    template_id = table.Column<int>(nullable: true),
                    content = table.Column<byte[]>(nullable: true),
                    content_type = table.Column<string>(maxLength: 100, nullable: false),
                    created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "project",
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
                    table.PrimaryKey("pk_project", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "resource",
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
                    table.PrimaryKey("pk_resource", x => x.id);
                    table.ForeignKey(
                        name: "fk_resource_project_project_id",
                        column: x => x.project_id,
                        principalTable: "project",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "template",
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
                    table.PrimaryKey("pk_template", x => x.id);
                    table.ForeignKey(
                        name: "fk_template_project_project_id",
                        column: x => x.project_id,
                        principalTable: "project",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "zone",
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
                    table.PrimaryKey("pk_zone", x => x.id);
                    table.ForeignKey(
                        name: "fk_zone_project_project_id",
                        column: x => x.project_id,
                        principalTable: "project",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "format",
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
                    table.PrimaryKey("pk_format", x => x.id);
                    table.ForeignKey(
                        name: "fk_format_template_template_id",
                        column: x => x.template_id,
                        principalTable: "template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_format_zone_zone_id",
                        column: x => x.zone_id,
                        principalTable: "zone",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "spooler",
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
                    table.PrimaryKey("pk_spooler", x => x.id);
                    table.ForeignKey(
                        name: "fk_spooler_zone_zone_id",
                        column: x => x.zone_id,
                        principalTable: "zone",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "terminal",
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
                    table.PrimaryKey("pk_terminal", x => x.id);
                    table.ForeignKey(
                        name: "fk_terminal_zone_zone_id",
                        column: x => x.zone_id,
                        principalTable: "zone",
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
                        name: "fk_zone_route_spooler_spooler_id",
                        column: x => x.spooler_id,
                        principalTable: "spooler",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_zone_route_zone_zone_id",
                        column: x => x.zone_id,
                        principalTable: "zone",
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
                        name: "fk_terminal_route_spooler_spooler_id",
                        column: x => x.spooler_id,
                        principalTable: "spooler",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_terminal_route_terminal_terminal_id",
                        column: x => x.terminal_id,
                        principalTable: "terminal",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_format_template_id",
                table: "format",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "ix_format_zone_id_alias",
                table: "format",
                columns: new[] { "zone_id", "alias" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_project_key",
                table: "project",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_resource_project_id_alias",
                table: "resource",
                columns: new[] { "project_id", "alias" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_spooler_key",
                table: "spooler",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_spooler_zone_id",
                table: "spooler",
                column: "zone_id");

            migrationBuilder.CreateIndex(
                name: "ix_template_project_id",
                table: "template",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_terminal_key",
                table: "terminal",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_terminal_zone_id",
                table: "terminal",
                column: "zone_id");

            migrationBuilder.CreateIndex(
                name: "ix_terminal_route_spooler_id",
                table: "terminal_route",
                column: "spooler_id");

            migrationBuilder.CreateIndex(
                name: "ix_terminal_route_terminal_id",
                table: "terminal_route",
                column: "terminal_id");

            migrationBuilder.CreateIndex(
                name: "ix_zone_project_id",
                table: "zone",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_zone_route_spooler_id",
                table: "zone_route",
                column: "spooler_id");

            migrationBuilder.CreateIndex(
                name: "ix_zone_route_zone_id",
                table: "zone_route",
                column: "zone_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document");

            migrationBuilder.DropTable(
                name: "format");

            migrationBuilder.DropTable(
                name: "resource");

            migrationBuilder.DropTable(
                name: "terminal_route");

            migrationBuilder.DropTable(
                name: "zone_route");

            migrationBuilder.DropTable(
                name: "template");

            migrationBuilder.DropTable(
                name: "terminal");

            migrationBuilder.DropTable(
                name: "spooler");

            migrationBuilder.DropTable(
                name: "zone");

            migrationBuilder.DropTable(
                name: "project");
        }
    }
}
