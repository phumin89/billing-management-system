using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerCompanyProfileIconMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IconContentType",
                table: "OwnerCompanyProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IconStorageKey",
                table: "OwnerCompanyProfiles",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_IconContentType_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: "[IconContentType] IS NULL OR LEN(TRIM(N' ' + NCHAR(9) + NCHAR(10) + NCHAR(11) + NCHAR(12) + NCHAR(13) + NCHAR(160) FROM [IconContentType])) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_IconStorageKey_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: "[IconStorageKey] IS NULL OR LEN(TRIM(N' ' + NCHAR(9) + NCHAR(10) + NCHAR(11) + NCHAR(12) + NCHAR(13) + NCHAR(160) FROM [IconStorageKey])) > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_OwnerCompanyProfiles_IconContentType_NotBlank",
                table: "OwnerCompanyProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OwnerCompanyProfiles_IconStorageKey_NotBlank",
                table: "OwnerCompanyProfiles");

            migrationBuilder.DropColumn(
                name: "IconContentType",
                table: "OwnerCompanyProfiles");

            migrationBuilder.DropColumn(
                name: "IconStorageKey",
                table: "OwnerCompanyProfiles");
        }
    }
}
