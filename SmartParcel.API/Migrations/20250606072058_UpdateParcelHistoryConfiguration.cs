using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParcel.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateParcelHistoryConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParcelHistory_Timestamp",
                table: "ParcelHistory");

            migrationBuilder.DropIndex(
                name: "IX_ParcelHistory_TrackingId",
                table: "ParcelHistory");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Timestamp",
                table: "ParcelHistory",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP AT TIME ZONE 'UTC'",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelHistory_TrackingId_Timestamp",
                table: "ParcelHistory",
                columns: new[] { "TrackingId", "Timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParcelHistory_TrackingId_Timestamp",
                table: "ParcelHistory");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Timestamp",
                table: "ParcelHistory",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelHistory_Timestamp",
                table: "ParcelHistory",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelHistory_TrackingId",
                table: "ParcelHistory",
                column: "TrackingId");
        }
    }
}
