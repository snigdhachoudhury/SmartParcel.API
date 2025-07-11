using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParcel.API.Migrations
{
    /// <inheritdoc />
    public partial class AddOTPFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryOTP",
                table: "Parcels",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOTPVerified",
                table: "Parcels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OTPGeneratedAt",
                table: "Parcels",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parcels_TrackingId",
                table: "Parcels",
                column: "TrackingId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Parcels_TrackingId",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "DeliveryOTP",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "IsOTPVerified",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "OTPGeneratedAt",
                table: "Parcels");
        }
    }
}
