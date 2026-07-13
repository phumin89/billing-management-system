using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HardenOwnerCompanyPersistenceInvariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF (SELECT COUNT_BIG(*) FROM [OwnerCompanyProfiles]) > 1
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: more than one profile exists. Remove duplicates before retrying.', 1;

                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(LTRIM(RTRIM([CompanyName]))) = 0)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: CompanyName contains blank values. Remove blank values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(LTRIM(RTRIM([AddressLine1]))) = 0)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: AddressLine1 contains blank values. Remove blank values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(LTRIM(RTRIM([CityProvinceState]))) = 0)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: CityProvinceState contains blank values. Remove blank values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(LTRIM(RTRIM([PostalCode]))) = 0)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: PostalCode contains blank values. Remove blank values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(LTRIM(RTRIM([Country]))) = 0)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: Country contains blank values. Remove blank values before retrying.', 1;

                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(LTRIM(RTRIM([CompanyName]))) > 200)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: CompanyName exceeds 200 characters. Shorten invalid values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(LTRIM(RTRIM([AddressLine1]))) > 300)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: AddressLine1 exceeds 300 characters. Shorten invalid values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(LTRIM(RTRIM([AddressLine2]))) > 300)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: AddressLine2 exceeds 300 characters. Shorten invalid values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(LTRIM(RTRIM([CityProvinceState]))) > 150)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: CityProvinceState exceeds 150 characters. Shorten invalid values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(LTRIM(RTRIM([PostalCode]))) > 50)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: PostalCode exceeds 50 characters. Shorten invalid values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(LTRIM(RTRIM([Country]))) > 100)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: Country exceeds 100 characters. Shorten invalid values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(LTRIM(RTRIM([TaxId]))) > 100)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: TaxId exceeds 100 characters. Shorten invalid values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(LTRIM(RTRIM([Phone]))) > 100)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: Phone exceeds 100 characters. Shorten invalid values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(LTRIM(RTRIM([Email]))) > 254)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: Email exceeds 254 characters. Shorten invalid values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(LTRIM(RTRIM([Website]))) > 300)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: Website exceeds 300 characters. Shorten invalid values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(LTRIM(RTRIM([LogoReference]))) > 500)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: LogoReference exceeds 500 characters. Shorten invalid values before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [OwnerCompanyProfiles] WHERE LEN(LTRIM(RTRIM([RegistrationNumber]))) > 100)
                    THROW 51000, 'OwnerCompanyProfiles migration blocked: RegistrationNumber exceeds 100 characters. Shorten invalid values before retrying.', 1;

                UPDATE [OwnerCompanyProfiles]
                SET [CompanyName] = LTRIM(RTRIM([CompanyName])),
                    [AddressLine1] = LTRIM(RTRIM([AddressLine1])),
                    [AddressLine2] = NULLIF(LTRIM(RTRIM([AddressLine2])), ''),
                    [CityProvinceState] = LTRIM(RTRIM([CityProvinceState])),
                    [PostalCode] = LTRIM(RTRIM([PostalCode])),
                    [Country] = LTRIM(RTRIM([Country])),
                    [TaxId] = NULLIF(LTRIM(RTRIM([TaxId])), ''),
                    [Phone] = NULLIF(LTRIM(RTRIM([Phone])), ''),
                    [Email] = NULLIF(LTRIM(RTRIM([Email])), ''),
                    [Website] = NULLIF(LTRIM(RTRIM([Website])), ''),
                    [LogoReference] = NULLIF(LTRIM(RTRIM([LogoReference])), ''),
                    [RegistrationNumber] = NULLIF(LTRIM(RTRIM([RegistrationNumber])), '');
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
                sql: "LEN(LTRIM(RTRIM([AddressLine1]))) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_AddressLine2_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: "[AddressLine2] IS NULL OR LEN(LTRIM(RTRIM([AddressLine2]))) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_CityProvinceState_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: "LEN(LTRIM(RTRIM([CityProvinceState]))) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_CompanyName_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: "LEN(LTRIM(RTRIM([CompanyName]))) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_Country_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: "LEN(LTRIM(RTRIM([Country]))) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_Email_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: "[Email] IS NULL OR LEN(LTRIM(RTRIM([Email]))) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_LogoReference_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: "[LogoReference] IS NULL OR LEN(LTRIM(RTRIM([LogoReference]))) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_Phone_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: "[Phone] IS NULL OR LEN(LTRIM(RTRIM([Phone]))) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_PostalCode_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: "LEN(LTRIM(RTRIM([PostalCode]))) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_RegistrationNumber_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: "[RegistrationNumber] IS NULL OR LEN(LTRIM(RTRIM([RegistrationNumber]))) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_SingletonKey",
                table: "OwnerCompanyProfiles",
                sql: "[SingletonKey] = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_TaxId_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: "[TaxId] IS NULL OR LEN(LTRIM(RTRIM([TaxId]))) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerCompanyProfiles_Website_NotBlank",
                table: "OwnerCompanyProfiles",
                sql: "[Website] IS NULL OR LEN(LTRIM(RTRIM([Website]))) > 0");
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
    }
}
