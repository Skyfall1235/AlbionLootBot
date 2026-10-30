using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlbionLootBot.Migrations
{
    /// <inheritdoc />
    public partial class GuildInfoAndPayouts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParticipantPayout_Players_PlayerId",
                table: "ParticipantPayout");

            migrationBuilder.AddColumn<int>(
                name: "ContributorId",
                table: "LootEntity",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LootEntity_ContributorId",
                table: "LootEntity",
                column: "ContributorId");

            migrationBuilder.AddForeignKey(
                name: "FK_LootEntity_Players_ContributorId",
                table: "LootEntity",
                column: "ContributorId",
                principalTable: "Players",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParticipantPayout_Players_PlayerId",
                table: "ParticipantPayout",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LootEntity_Players_ContributorId",
                table: "LootEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_ParticipantPayout_Players_PlayerId",
                table: "ParticipantPayout");

            migrationBuilder.DropIndex(
                name: "IX_LootEntity_ContributorId",
                table: "LootEntity");

            migrationBuilder.DropColumn(
                name: "ContributorId",
                table: "LootEntity");

            migrationBuilder.AddForeignKey(
                name: "FK_ParticipantPayout_Players_PlayerId",
                table: "ParticipantPayout",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
