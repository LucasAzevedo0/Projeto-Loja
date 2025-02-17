using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaRemastered.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionUserName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RelatedUserName",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelatedUserName",
                table: "Transactions");
        }
    }
}
