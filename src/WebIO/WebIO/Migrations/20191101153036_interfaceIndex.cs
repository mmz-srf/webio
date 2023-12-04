namespace Tpc.WebIO.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class interfaceIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "Interfaces",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Index",
                table: "Interfaces");
        }
    }
}
