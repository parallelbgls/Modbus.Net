using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MachineJob.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DatabaseWrites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Value1 = table.Column<double>(type: "double", nullable: true),
                    Value2 = table.Column<double>(type: "double", nullable: true),
                    Value3 = table.Column<double>(type: "double", nullable: true),
                    Value4 = table.Column<double>(type: "double", nullable: true),
                    Value5 = table.Column<double>(type: "double", nullable: true),
                    Value6 = table.Column<double>(type: "double", nullable: true),
                    Value7 = table.Column<double>(type: "double", nullable: true),
                    Value8 = table.Column<double>(type: "double", nullable: true),
                    Value9 = table.Column<double>(type: "double", nullable: true),
                    Value10 = table.Column<double>(type: "double", nullable: true),
                    UpdateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseWrites", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DatabaseWrites");
        }
    }
}
