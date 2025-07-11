using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParcel.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAfterPricingFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parcels_PricingTiers_PricingTierId",
                table: "Parcels");

            migrationBuilder.AlterColumn<decimal>(
                name: "PricePerKg",
                table: "PricingTiers",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "BasePrice",
                table: "PricingTiers",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "ShippingCost",
                table: "Parcels",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddForeignKey(
                name: "FK_Parcels_PricingTiers_PricingTierId",
                table: "Parcels",
                column: "PricingTierId",
                principalTable: "PricingTiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.InsertData(
                table: "PricingTiers",
                columns: new[] { "Name", "BasePrice", "PricePerKg", "EstimatedDeliveryDays", "IsActive", "Description" },
                values: new object[] { "Standard Delivery", 5.00m, 1.50m, 4, true, "Regular delivery within 3-5 business days" });

            migrationBuilder.InsertData(
                table: "PricingTiers",
                columns: new[] { "Name", "BasePrice", "PricePerKg", "EstimatedDeliveryDays", "IsActive", "Description" },
                values: new object[] { "Express Delivery", 12.00m, 3.00m, 2, true, "Fast delivery within 1-2 business days" });

            migrationBuilder.InsertData(
                table: "PricingTiers",
                columns: new[] { "Name", "BasePrice", "PricePerKg", "EstimatedDeliveryDays", "IsActive", "Description" },
                values: new object[] { "Economy Delivery", 3.00m, 1.00m, 7, true, "Budget-friendly option with 5-7 business days delivery" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Delete seed data
            migrationBuilder.DeleteData(
                table: "PricingTiers",
                keyColumn: "Name",
                keyValues: new object[] { "Standard Delivery", "Express Delivery", "Economy Delivery" });

            migrationBuilder.DropForeignKey(
                name: "FK_Parcels_PricingTiers_PricingTierId",
                table: "Parcels");

            migrationBuilder.AlterColumn<decimal>(
                name: "PricePerKg",
                table: "PricingTiers",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "BasePrice",
                table: "PricingTiers",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ShippingCost",
                table: "Parcels",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_Parcels_PricingTiers_PricingTierId",
                table: "Parcels",
                column: "PricingTierId",
                principalTable: "PricingTiers",
                principalColumn: "Id");
        }
    }
}
