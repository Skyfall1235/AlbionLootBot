using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlbionLootBot.Migrations
{
    /// <inheritdoc />
    public partial class SyncModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Calculations_CalculationId",
                table: "Calculations");

            migrationBuilder.DropColumn(
                name: "CalculationId",
                table: "Calculations");

            migrationBuilder.DropColumn(
                name: "PerPersonGrossShare",
                table: "Calculations");

            migrationBuilder.CreateIndex(
                name: "IX_Calculations_Id",
                table: "Calculations",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Calculations_Id",
                table: "Calculations");

            migrationBuilder.AddColumn<string>(
                name: "CalculationId",
                table: "Calculations",
                type: "TEXT",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PerPersonGrossShare",
                table: "Calculations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Calculations_CalculationId",
                table: "Calculations",
                column: "CalculationId",
                unique: true);
        }
    }
}
