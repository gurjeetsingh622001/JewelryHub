using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewelryHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerAddressSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "CustomerAddresses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "CustomerAddresses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CustomerAddresses",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "CustomerAddresses");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "CustomerAddresses");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CustomerAddresses");
        }
    }
}
