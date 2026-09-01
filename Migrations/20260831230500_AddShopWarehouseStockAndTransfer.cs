using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmartStock.Data;

#nullable disable

namespace SmartStock.Migrations
{
    [DbContext(typeof(SmartStockDbContext))]
    [Migration("20260831230500_AddShopWarehouseStockAndTransfer")]
    public partial class AddShopWarehouseStockAndTransfer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShopStock",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseStock",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE \"Products\" SET \"ShopStock\" = \"CurrentStock\", \"WarehouseStock\" = 0 WHERE \"ShopStock\" = 0 AND \"WarehouseStock\" = 0;");

            migrationBuilder.AddColumn<string>(
                name: "DelegatePerson",
                table: "StockMovements",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ShopStock", table: "Products");
            migrationBuilder.DropColumn(name: "WarehouseStock", table: "Products");
            migrationBuilder.DropColumn(name: "DelegatePerson", table: "StockMovements");
        }
    }
}
