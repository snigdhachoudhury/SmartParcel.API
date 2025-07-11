using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartParcel.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PricingTierId",
                table: "Parcels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingCost",
                table: "Parcels",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "PricingTiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    BasePrice = table.Column<decimal>(type: "numeric", nullable: false),
                    PricePerKg = table.Column<decimal>(type: "numeric", nullable: false),
                    EstimatedDeliveryDays = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingTiers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Parcels_PricingTierId",
                table: "Parcels",
                column: "PricingTierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parcels_PricingTiers_PricingTierId",
                table: "Parcels",
                column: "PricingTierId",
                principalTable: "PricingTiers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parcels_PricingTiers_PricingTierId",
                table: "Parcels");

            migrationBuilder.DropTable(
                name: "PricingTiers");

            migrationBuilder.DropIndex(
                name: "IX_Parcels_PricingTierId",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "PricingTierId",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "ShippingCost",
                table: "Parcels");
        }
    }
}
