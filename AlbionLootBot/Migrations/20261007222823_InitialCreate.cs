using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlbionLootBot.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LootSplit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SessionName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LootSplit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DiscordName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DiscordPlayerId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Calculations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LootsplitId = table.Column<int>(type: "INTEGER", nullable: false),
                    CalculationId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SilverBagTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ItemTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RecipientCount = table.Column<int>(type: "INTEGER", nullable: false),
                    TaxRatePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TotalTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetTotalAfterTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PerPersonGrossShare = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PerPersonNetShare = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remainder = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calculations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Calculations_LootSplit_LootsplitId",
                        column: x => x.LootsplitId,
                        principalTable: "LootSplit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LootEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LootsplitId = table.Column<int>(type: "INTEGER", nullable: false),
                    TaxableEntity = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ValueSilver = table.Column<long>(type: "INTEGER", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LootEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LootEntity_LootSplit_LootsplitId",
                        column: x => x.LootsplitId,
                        principalTable: "LootSplit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LootsplitId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Participants_LootSplit_LootsplitId",
                        column: x => x.LootsplitId,
                        principalTable: "LootSplit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Participants_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Calculations_CalculationId",
                table: "Calculations",
                column: "CalculationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Calculations_LootsplitId",
                table: "Calculations",
                column: "LootsplitId");

            migrationBuilder.CreateIndex(
                name: "IX_LootEntity_LootsplitId",
                table: "LootEntity",
                column: "LootsplitId");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_LootsplitId_PlayerId",
                table: "Participants",
                columns: new[] { "LootsplitId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Participants_PlayerId",
                table: "Participants",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_DiscordPlayerId",
                table: "Players",
                column: "DiscordPlayerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_Name",
                table: "Players",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Calculations");

            migrationBuilder.DropTable(
                name: "LootEntity");

            migrationBuilder.DropTable(
                name: "Participants");

            migrationBuilder.DropTable(
                name: "LootSplit");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
