using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.Services.ProductAPI.Migrations
{
    public partial class AddImageUrlNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "CategoryName", "Description", "ImageUrl", "Name", "Price" },
                values: new object[,]
                {
                    { 1, "Хлебо-булочные изделия", "Лучшая самса.", null, "Самса с говядиной", 15.0 },
                    { 2, "Хлебо-булочные изделия", "Поистине вкусный и хороший пиродок.", null, "Пирожок с луком", 13.99 },
                    { 3, "Пицы", "Новая разновидность пицы на рынке", null, "Закрытая пица", 10.99 },
                    { 4, "Пицы", "Классический вариант пицы.", null, "Открытая пица", 15.0 }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 4);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
