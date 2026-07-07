using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameOwnerCompanyCityProvinceState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "City",
                table: "OwnerCompanyProfiles",
                newName: "CityProvinceState");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CityProvinceState",
                table: "OwnerCompanyProfiles",
                newName: "City");
        }
    }
}
