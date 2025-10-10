using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WastageUploadService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Wastages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InwardChallanId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PartyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VehicleNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SlipNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NetWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MouReportJson = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                    ImageUrlsJson = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wastages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wastages_InwardChallanId",
                table: "Wastages",
                column: "InwardChallanId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Wastages");
        }
    }
}
