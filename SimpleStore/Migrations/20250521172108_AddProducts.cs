using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SimpleStore.Migrations
{
    /// <inheritdoc />
    public partial class AddProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "ImageUrl", "IsActive", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { 1, "Electronics", new DateTime(2025, 5, 21, 20, 51, 8, 515, DateTimeKind.Local).AddTicks(6048), "High-performance laptop with 16GB RAM, 512GB SSD, and a powerful processor for all your computing needs.", "/images/products/laptop-x1.jpg", true, "Laptop Pro X1", 1299.99m, 15 },
                    { 2, "Phones", new DateTime(2025, 5, 21, 20, 51, 8, 515, DateTimeKind.Local).AddTicks(6051), "Latest smartphone with 6.5-inch AMOLED display, 128GB storage, and an impressive camera system.", "/images/products/galaxy-s.jpg", true, "Smartphone Galaxy S", 899.99m, 25 },
                    { 3, "Audio", new DateTime(2025, 5, 21, 20, 51, 8, 515, DateTimeKind.Local).AddTicks(6053), "Premium wireless headphones with noise cancellation and 30-hour battery life.", "/images/products/headphones.jpg", true, "Wireless Headphones", 249.99m, 30 },
                    { 4, "Wearables", new DateTime(2025, 5, 21, 20, 51, 8, 515, DateTimeKind.Local).AddTicks(6055), "Track your fitness, receive notifications, and more with this sleek smart watch.", "/images/products/smartwatch.jpg", true, "Smart Watch Series 5", 349.99m, 20 },
                    { 5, "Electronics", new DateTime(2025, 5, 21, 20, 51, 8, 515, DateTimeKind.Local).AddTicks(6057), "12-inch tablet with retina display, perfect for work and entertainment on the go.", "/images/products/tablet-pro.jpg", true, "Tablet Pro 12", 799.99m, 10 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
