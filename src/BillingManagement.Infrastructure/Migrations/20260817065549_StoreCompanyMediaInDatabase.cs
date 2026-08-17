using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StoreCompanyMediaInDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyMedia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Length = table.Column<long>(type: "bigint", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyMedia", x => x.Id);
                    table.CheckConstraint("CK_CompanyMedia_Length", "[Length] = DATALENGTH([Content])");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyMedia");
        }
    }
}
