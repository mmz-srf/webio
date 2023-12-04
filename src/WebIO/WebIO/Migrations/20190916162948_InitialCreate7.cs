namespace Tpc.WebIO.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class InitialCreate7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceProperties_Devices_DeviceEntityId",
                table: "DeviceProperties");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Devices",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "DeviceProperties",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "DeviceEntityId",
                table: "DeviceProperties",
                newName: "DeviceId");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceProperties_DeviceEntityId",
                table: "DeviceProperties",
                newName: "IX_DeviceProperties_DeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceProperties_Devices_DeviceId",
                table: "DeviceProperties",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceProperties_Devices_DeviceId",
                table: "DeviceProperties");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Devices",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "DeviceProperties",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "DeviceId",
                table: "DeviceProperties",
                newName: "DeviceEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceProperties_DeviceId",
                table: "DeviceProperties",
                newName: "IX_DeviceProperties_DeviceEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceProperties_Devices_DeviceEntityId",
                table: "DeviceProperties",
                column: "DeviceEntityId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
