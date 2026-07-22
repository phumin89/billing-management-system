using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerCompanyProfileCoverMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverContentType",
                table: "OwnerCompanyProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverStorageKey",
                table: "OwnerCompanyProfiles",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_CoverContentType_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: "[CoverContentType] IS NULL OR LEN(TRIM(N' ' + NCHAR(9) + NCHAR(10) + NCHAR(11) + NCHAR(12) + NCHAR(13) + NCHAR(160) FROM [CoverContentType])) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_CoverStorageKey_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: "[CoverStorageKey] IS NULL OR LEN(TRIM(N' ' + NCHAR(9) + NCHAR(10) + NCHAR(11) + NCHAR(12) + NCHAR(13) + NCHAR(160) FROM [CoverStorageKey])) > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_OwnerCompanyProfiles_CoverContentType_NotBlank",
                table: "OwnerCompanyProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OwnerCompanyProfiles_CoverStorageKey_NotBlank",
                table: "OwnerCompanyProfiles");

            migrationBuilder.DropColumn(
                name: "CoverContentType",
                table: "OwnerCompanyProfiles");

            migrationBuilder.DropColumn(
                name: "CoverStorageKey",
                table: "OwnerCompanyProfiles");
        }
    }
}
