using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WastageUploadService.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNetWeightAndSlipNo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NetWeight",
                table: "Wastages");

            migrationBuilder.DropColumn(
                name: "SlipNo",
                table: "Wastages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "NetWeight",
                table: "Wastages",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SlipNo",
                table: "Wastages",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
