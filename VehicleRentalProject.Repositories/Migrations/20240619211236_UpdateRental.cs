using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleRentalProject.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRental : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PenaltyAmount",
                table: "Rentals",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PenaltyAmount",
                table: "Rentals");
        }
    }
}
