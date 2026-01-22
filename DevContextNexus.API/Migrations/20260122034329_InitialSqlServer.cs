using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevContextNexus.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "context_files",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    file_path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    last_sha = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    public_url = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_context_files", x => x.id);
                    table.ForeignKey(
                        name: "FK_context_files_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_context_files_project_id",
                table: "context_files",
                column: "project_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "context_files");

            migrationBuilder.DropTable(
                name: "projects");
        }
    }
}
