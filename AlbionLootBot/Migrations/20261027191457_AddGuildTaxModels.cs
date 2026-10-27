using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlbionLootBot.Migrations
{
    /// <inheritdoc />
    public partial class AddGuildTaxModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AOGuildInfoId",
                table: "Players",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AOGuildInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlbionOnlineGuildName = table.Column<string>(type: "TEXT", nullable: false),
                    DiscordGuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    SilverTaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ItemValueTaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ResourceTaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AOGuildInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParticipantPayout",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CalculationEntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AOGuildInfoId = table.Column<int>(type: "INTEGER", nullable: true),
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: false),
                    GrossShare = table.Column<long>(type: "INTEGER", nullable: false),
                    NetShare = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantPayout", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParticipantPayout_AOGuildInfo_AOGuildInfoId",
                        column: x => x.AOGuildInfoId,
                        principalTable: "AOGuildInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ParticipantPayout_Calculations_CalculationEntityId",
                        column: x => x.CalculationEntityId,
                        principalTable: "Calculations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParticipantPayout_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuildTaxEntry",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParticipantPayoutId = table.Column<int>(type: "INTEGER", nullable: false),
                    AOGuildInfoId = table.Column<int>(type: "INTEGER", nullable: false),
                    TaxType = table.Column<int>(type: "INTEGER", nullable: false),
                    TaxableAmountSilver = table.Column<long>(type: "INTEGER", nullable: false),
                    RateApplied = table.Column<decimal>(type: "TEXT", nullable: false),
                    TaxAmountSilver = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildTaxEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildTaxEntry_AOGuildInfo_AOGuildInfoId",
                        column: x => x.AOGuildInfoId,
                        principalTable: "AOGuildInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GuildTaxEntry_ParticipantPayout_ParticipantPayoutId",
                        column: x => x.ParticipantPayoutId,
                        principalTable: "ParticipantPayout",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Players_AOGuildInfoId",
                table: "Players",
                column: "AOGuildInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_AOGuildInfo_DiscordGuildId",
                table: "AOGuildInfo",
                column: "DiscordGuildId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuildTaxEntry_AOGuildInfoId_PaidAt",
                table: "GuildTaxEntry",
                columns: new[] { "AOGuildInfoId", "PaidAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GuildTaxEntry_ParticipantPayoutId",
                table: "GuildTaxEntry",
                column: "ParticipantPayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantPayout_AOGuildInfoId",
                table: "ParticipantPayout",
                column: "AOGuildInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantPayout_CalculationEntityId",
                table: "ParticipantPayout",
                column: "CalculationEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantPayout_PlayerId",
                table: "ParticipantPayout",
                column: "PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_AOGuildInfo_AOGuildInfoId",
                table: "Players",
                column: "AOGuildInfoId",
                principalTable: "AOGuildInfo",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_AOGuildInfo_AOGuildInfoId",
                table: "Players");

            migrationBuilder.DropTable(
                name: "GuildTaxEntry");

            migrationBuilder.DropTable(
                name: "ParticipantPayout");

            migrationBuilder.DropTable(
                name: "AOGuildInfo");

            migrationBuilder.DropIndex(
                name: "IX_Players_AOGuildInfoId",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "AOGuildInfoId",
                table: "Players");
        }
    }
}
