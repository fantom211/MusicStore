using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MusicStore.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ItemsTotal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    SpinDiscountPercent = table.Column<int>(type: "integer", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Address_City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Address_Street = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Address_House = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Address_Apartment = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Address_PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Address_RecipientName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address_Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    DiscountPercent = table.Column<int>(type: "integer", nullable: false),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailySpins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SpinDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DiscountPercent = table.Column<int>(type: "integer", nullable: false),
                    SymbolsJson = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    UsedInOrderId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailySpins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailySpins_Orders_UsedInOrderId",
                        column: x => x.UsedInOrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "DiscountPercent", "ImageUrl", "IsActive", "Name", "Price", "StockQuantity" },
                values: new object[,]
                {
                    { 1, "Guitars", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Электрогитара, клён, 3 сингла. Классический звук для рока и блюза.", 10, "/images/products/guitars.svg", true, "Fender Player Stratocaster", 85000m, 5 },
                    { 2, "Guitars", new DateTime(2026, 1, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Электрогитара для начинающих и продолжающих, хамбакер + 2 сингла.", 0, "/images/products/guitars.svg", true, "Yamaha Pacifica 112V", 42000m, 8 },
                    { 3, "Strings", new DateTime(2026, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), "Струны для электрогитары, никель, комплект.", 0, "/images/products/strings.svg", true, "Ernie Ball Regular Slinky (10-46)", 850m, 40 },
                    { 4, "Strings", new DateTime(2026, 1, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Струны для акустической гитары, фосфорная бронза.", 15, "/images/products/strings.svg", true, "D'Addario EJ16 Phosphor Bronze (12-53)", 1100m, 25 },
                    { 5, "Pianos", new DateTime(2026, 1, 14, 0, 0, 0, 0, DateTimeKind.Utc), "Цифровое пианино, 88 взвешенных клавиш, звук рояля CFIIIS.", 5, "/images/products/pianos.svg", true, "Yamaha P-125a", 98000m, 4 },
                    { 6, "Pianos", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Синтезатор для обучения, 61 клавиша, режим пошагового обучения.", 0, "/images/products/pianos.svg", true, "Casio CTK-3500", 32000m, 10 },
                    { 7, "Drums", new DateTime(2026, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc), "Ударная установка 5 барабанов с тарелками и стойками.", 0, "/images/products/drums.svg", true, "Pearl Roadshow RS525SC", 65000m, 3 },
                    { 8, "Drums", new DateTime(2026, 1, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Барабанные палочки, гикори, самая популярная модель.", 0, "/images/products/drums.svg", true, "Vic Firth American Classic 5A", 1500m, 50 },
                    { 9, "Vinyl", new DateTime(2026, 1, 18, 0, 0, 0, 0, DateTimeKind.Utc), "Классический виниловый альбом, 180 грамм.", 20, "/images/products/vinyl.svg", true, "Pink Floyd — The Dark Side of the Moon (LP)", 3500m, 15 },
                    { 10, "Vinyl", new DateTime(2026, 1, 19, 0, 0, 0, 0, DateTimeKind.Utc), "Электронный классик на виниле, двойной альбом.", 0, "/images/products/vinyl.svg", true, "Daft Punk — Discovery (LP)", 2800m, 12 },
                    { 11, "Amplifiers", new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Комбоусилитель для электрогитары, 15 Вт, реверб.", 10, "/images/products/amplifiers.svg", true, "Marshall MG15G", 28000m, 6 },
                    { 12, "Amplifiers", new DateTime(2026, 1, 21, 0, 0, 0, 0, DateTimeKind.Utc), "Гитарный комбоусилитель, 40 Вт, 12 эффектов.", 0, "/images/products/amplifiers.svg", true, "Fender Champion 40", 32000m, 7 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductId",
                table: "CartItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_UserId_ProductId",
                table: "CartItems",
                columns: new[] { "UserId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailySpins_UsedInOrderId",
                table: "DailySpins",
                column: "UsedInOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_DailySpins_UserId_SpinDate",
                table: "DailySpins",
                columns: new[] { "UserId", "SpinDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Category",
                table: "Products",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive",
                table: "Products",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "DailySpins");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
