using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleStore.Migrations
{
    /// <inheritdoc />
    public partial class OrderManagment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CancellationRequested",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ReturnRequested",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "OrderItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationRequested",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReturnRequested",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 22, 14, 48, 19, 734, DateTimeKind.Local).AddTicks(3595));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 22, 14, 48, 19, 734, DateTimeKind.Local).AddTicks(3598));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 22, 14, 48, 19, 734, DateTimeKind.Local).AddTicks(3601));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 22, 14, 48, 19, 734, DateTimeKind.Local).AddTicks(3603));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 22, 14, 48, 19, 734, DateTimeKind.Local).AddTicks(3605));
        }
    }
}
