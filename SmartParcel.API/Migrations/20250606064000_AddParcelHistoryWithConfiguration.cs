using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartParcel.API.Migrations
{
    /// <inheritdoc />
    public partial class AddParcelHistoryWithConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_Parcels_TrackingId",
                table: "Parcels",
                column: "TrackingId");

            migrationBuilder.CreateTable(
                name: "ParcelHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrackingId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    HandledBy = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParcelHistory_Parcels_TrackingId",
                        column: x => x.TrackingId,
                        principalTable: "Parcels",
                        principalColumn: "TrackingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParcelHistory_Timestamp",
                table: "ParcelHistory",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelHistory_TrackingId",
                table: "ParcelHistory",
                column: "TrackingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParcelHistory");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Parcels_TrackingId",
                table: "Parcels");
        }
    }
}
