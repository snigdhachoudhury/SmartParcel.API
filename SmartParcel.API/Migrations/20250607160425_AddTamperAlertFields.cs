using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParcel.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTamperAlertFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TrackingId",
                table: "TamperAlerts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "TamperAlerts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsResolved",
                table: "TamperAlerts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "TamperAlerts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportedBy",
                table: "TamperAlerts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Resolution",
                table: "TamperAlerts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAt",
                table: "TamperAlerts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResolvedBy",
                table: "TamperAlerts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsResolved",
                table: "TamperAlerts");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "TamperAlerts");

            migrationBuilder.DropColumn(
                name: "ReportedBy",
                table: "TamperAlerts");

            migrationBuilder.DropColumn(
                name: "Resolution",
                table: "TamperAlerts");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "TamperAlerts");

            migrationBuilder.DropColumn(
                name: "ResolvedBy",
                table: "TamperAlerts");

            migrationBuilder.AlterColumn<string>(
                name: "TrackingId",
                table: "TamperAlerts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "TamperAlerts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
