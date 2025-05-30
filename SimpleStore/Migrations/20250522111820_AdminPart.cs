using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleStore.Migrations
{
    /// <inheritdoc />
    public partial class AdminPart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 21, 21, 44, 19, 122, DateTimeKind.Local).AddTicks(4334));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 21, 21, 44, 19, 122, DateTimeKind.Local).AddTicks(4338));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 21, 21, 44, 19, 122, DateTimeKind.Local).AddTicks(4340));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 21, 21, 44, 19, 122, DateTimeKind.Local).AddTicks(4342));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 21, 21, 44, 19, 122, DateTimeKind.Local).AddTicks(4344));
        }
    }
}
