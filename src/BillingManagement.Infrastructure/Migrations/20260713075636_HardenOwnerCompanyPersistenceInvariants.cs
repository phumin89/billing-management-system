using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HardenOwnerCompanyPersistenceInvariants : Migration
    {
        private const string SqlWhitespaceCharacters =
            "N' ' + NCHAR(9) + NCHAR(10) + NCHAR(11) + NCHAR(12) + NCHAR(13) + NCHAR(160)";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @Whitespace nvarchar(7) = N' ' + NCHAR(9) + NCHAR(10) + NCHAR(11) + NCHAR(12) + NCHAR(13) + NCHAR(160);

                IF (SELECT COUNT_BIG(*) FROM [OwnerCompanyProfiles]) > 1
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: more than one profile exists. Remove duplicates before retrying.', 1;

                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(TRIM(@Whitespace FROM [CompanyName])) = 0)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: CompanyName contains blank values. Remove blank values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(TRIM(@Whitespace FROM [AddressLine1])) = 0)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: AddressLine1 contains blank values. Remove blank values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(TRIM(@Whitespace FROM [CityProvinceState])) = 0)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: CityProvinceState contains blank values. Remove blank values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(TRIM(@Whitespace FROM [PostalCode])) = 0)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: PostalCode contains blank values. Remove blank values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(TRIM(@Whitespace FROM [Country])) = 0)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: Country contains blank values. Remove blank values before retrying.', 1;

                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(TRIM(@Whitespace FROM [CompanyName])) > 200)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: CompanyName exceeds 200 characters. Shorten invalid values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(TRIM(@Whitespace FROM [AddressLine1])) > 300)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: AddressLine1 exceeds 300 characters. Shorten invalid values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(TRIM(@Whitespace FROM [AddressLine2])) > 300)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: AddressLine2 exceeds 300 characters. Shorten invalid values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(TRIM(@Whitespace FROM [CityProvinceState])) > 150)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: CityProvinceState exceeds 150 characters. Shorten invalid values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(TRIM(@Whitespace FROM [PostalCode])) > 50)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: PostalCode exceeds 50 characters. Shorten invalid values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(TRIM(@Whitespace FROM [Country])) > 100)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: Country exceeds 100 characters. Shorten invalid values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(TRIM(@Whitespace FROM [TaxId])) > 100)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: TaxId exceeds 100 characters. Shorten invalid values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(TRIM(@Whitespace FROM [Phone])) > 100)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: Phone exceeds 100 characters. Shorten invalid values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(TRIM(@Whitespace FROM [Email])) > 254)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: Email exceeds 254 characters. Shorten invalid values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(TRIM(@Whitespace FROM [Website])) > 300)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: Website exceeds 300 characters. Shorten invalid values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(TRIM(@Whitespace FROM [LogoReference])) > 500)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: LogoReference exceeds 500 characters. Shorten invalid values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(TRIM(@Whitespace FROM [RegistrationNumber])) > 100)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: RegistrationNumber exceeds 100 characters. Shorten invalid values before retrying.', 1;

                UPDATE [OwnerCompanyProfiles]
                SET [CompanyName] = TRIM(@Whitespace FROM [CompanyName]),
                    [AddressLine1] = TRIM(@Whitespace FROM [AddressLine1]),
                    [AddressLine2] = NULLIF(TRIM(@Whitespace FROM [AddressLine2]), ''),
                    [CityProvinceState] = TRIM(@Whitespace FROM [CityProvinceState]),
                    [PostalCode] = TRIM(@Whitespace FROM [PostalCode]),
                    [Country] = TRIM(@Whitespace FROM [Country]),
                    [TaxId] = NULLIF(TRIM(@Whitespace FROM [TaxId]), ''),
                    [Phone] = NULLIF(TRIM(@Whitespace FROM [Phone]), ''),
                    [Email] = NULLIF(TRIM(@Whitespace FROM [Email]), ''),
                    [Website] = NULLIF(TRIM(@Whitespace FROM [Website]), ''),
                    [LogoReference] = NULLIF(TRIM(@Whitespace FROM [LogoReference]), ''),
                    [RegistrationNumber] = NULLIF(TRIM(@Whitespace FROM [RegistrationNumber]), '');
                """);

            migrationBuilder.AddColumn<byte>(
                name: "SingletonKey",
                table: "OwnerCompanyProfiles",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1);

            migrationBuilder.CreateIndex(
                name: "IX_OwnerCompanyProfiles_SingletonKey",
                table: "OwnerCompanyProfiles",
                column: "SingletonKey",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_AddressLine1_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: RequiredNotBlank("AddressLine1"));

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_AddressLine2_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: OptionalNotBlank("AddressLine2"));

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_CityProvinceState_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: RequiredNotBlank("CityProvinceState"));

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_CompanyName_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: RequiredNotBlank("CompanyName"));

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_Country_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: RequiredNotBlank("Country"));

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_Email_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: OptionalNotBlank("Email"));

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_LogoReference_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: OptionalNotBlank("LogoReference"));

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_Phone_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: OptionalNotBlank("Phone"));

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_PostalCode_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: RequiredNotBlank("PostalCode"));

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_RegistrationNumber_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: OptionalNotBlank("RegistrationNumber"));

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_SingletonKey",
                table: "OwnerCompanyProfiles",
                sql: "[SingletonKey] = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_TaxId_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: OptionalNotBlank("TaxId"));

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_Website_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: OptionalNotBlank("Website"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OwnerCompanyProfiles_SingletonKey",
                table: "OwnerCompanyProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OwnerCompanyProfiles_AddressLine1_NotBlank",
                table: "OwnerCompanyProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OwnerCompanyProfiles_AddressLine2_NotBlank",
                table: "OwnerCompanyProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OwnerCompanyProfiles_CityProvinceState_NotBlank",
                table: "OwnerCompanyProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OwnerCompanyProfiles_CompanyName_NotBlank",
                table: "OwnerCompanyProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OwnerCompanyProfiles_Country_NotBlank",
                table: "OwnerCompanyProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OwnerCompanyProfiles_Email_NotBlank",
                table: "OwnerCompanyProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OwnerCompanyProfiles_LogoReference_NotBlank",
                table: "OwnerCompanyProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OwnerCompanyProfiles_Phone_NotBlank",
                table: "OwnerCompanyProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OwnerCompanyProfiles_PostalCode_NotBlank",
                table: "OwnerCompanyProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OwnerCompanyProfiles_RegistrationNumber_NotBlank",
                table: "OwnerCompanyProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OwnerCompanyProfiles_SingletonKey",
                table: "OwnerCompanyProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OwnerCompanyProfiles_TaxId_NotBlank",
                table: "OwnerCompanyProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OwnerCompanyProfiles_Website_NotBlank",
                table: "OwnerCompanyProfiles");

            migrationBuilder.DropColumn(
                name: "SingletonKey",
                table: "OwnerCompanyProfiles");
        }

        private static string RequiredNotBlank(string columnName) =>
            $"LEN(TRIM({SqlWhitespaceCharacters} FROM [{columnName}])) > 0";

        private static string OptionalNotBlank(string columnName) =>
            $"[{columnName}] IS NULL OR {RequiredNotBlank(columnName)}";
    }
}
