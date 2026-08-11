using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingDocumentSellerSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SellerAddress",
                table: "Quotations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SellerCompanyName",
                table: "Quotations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SellerEmail",
                table: "Quotations",
                type: "nvarchar(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerPhone",
                table: "Quotations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerRegistrationNumber",
                table: "Quotations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerTaxId",
                table: "Quotations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerWebsite",
                table: "Quotations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerAddress",
                table: "Invoices",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SellerCompanyName",
                table: "Invoices",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SellerEmail",
                table: "Invoices",
                type: "nvarchar(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerPhone",
                table: "Invoices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerRegistrationNumber",
                table: "Invoices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerTaxId",
                table: "Invoices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerWebsite",
                table: "Invoices",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE q
                SET SellerCompanyName = COALESCE(p.CompanyName, N'Unknown seller'),
                    SellerAddress = COALESCE(NULLIF(CONCAT_WS(N', ', p.AddressLine1, p.AddressLine2, p.CityProvinceState, p.PostalCode, p.Country), N''), N'Not available'),
                    SellerTaxId = p.TaxId,
                    SellerPhone = p.Phone,
                    SellerEmail = p.Email,
                    SellerWebsite = p.Website,
                    SellerRegistrationNumber = p.RegistrationNumber
                FROM Quotations q
                LEFT JOIN OwnerCompanyProfiles p ON p.SingletonKey = 1;

                UPDATE i
                SET SellerCompanyName = q.SellerCompanyName,
                    SellerAddress = q.SellerAddress,
                    SellerTaxId = q.SellerTaxId,
                    SellerPhone = q.SellerPhone,
                    SellerEmail = q.SellerEmail,
                    SellerWebsite = q.SellerWebsite,
                    SellerRegistrationNumber = q.SellerRegistrationNumber
                FROM Invoices i
                INNER JOIN Quotations q ON q.Id = i.QuotationId;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SellerAddress",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "SellerCompanyName",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "SellerEmail",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "SellerPhone",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "SellerRegistrationNumber",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "SellerTaxId",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "SellerWebsite",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "SellerAddress",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SellerCompanyName",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SellerEmail",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SellerPhone",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SellerRegistrationNumber",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SellerTaxId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SellerWebsite",
                table: "Invoices");
        }
    }
}
