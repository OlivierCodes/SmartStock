using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStock.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixStaticSeedHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$hzikGu6iVUijYkgIw5o1qO/zMEiBpRb5SVHZdJvqjLSBpjkd4VBnu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$ub3dsRuHLBEJyK952RdAMOguRsZ5S9qTZEeigCt1InYLtgKdv1yXq");
        }
    }
}
