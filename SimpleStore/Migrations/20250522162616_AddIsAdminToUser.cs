using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleStore.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAdminToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 22, 19, 56, 15, 944, DateTimeKind.Local).AddTicks(5305));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 22, 19, 56, 15, 944, DateTimeKind.Local).AddTicks(5309));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 22, 19, 56, 15, 944, DateTimeKind.Local).AddTicks(5312));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 22, 19, 56, 15, 944, DateTimeKind.Local).AddTicks(5314));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 22, 19, 56, 15, 944, DateTimeKind.Local).AddTicks(5316));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 22, 15, 17, 36, 959, DateTimeKind.Local).AddTicks(3053));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 22, 15, 17, 36, 959, DateTimeKind.Local).AddTicks(3056));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 22, 15, 17, 36, 959, DateTimeKind.Local).AddTicks(3058));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 22, 15, 17, 36, 959, DateTimeKind.Local).AddTicks(3060));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 22, 15, 17, 36, 959, DateTimeKind.Local).AddTicks(3061));
        }
    }
}
