#nullable disable

namespace Tpc.WebIO.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class AddUniqueKeyIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Streams_InterfaceId_Name",
                table: "Streams",
                columns: new[] { "InterfaceId", "Name" },
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Interfaces_DeviceId_Name",
                table: "Interfaces",
                columns: new[] { "DeviceId", "Name" },
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_Name",
                table: "Devices",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeLog_Timestamp",
                table: "ChangeLog",
                column: "Timestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Streams_InterfaceId_Name",
                table: "Streams");

            migrationBuilder.DropIndex(
                name: "IX_Interfaces_DeviceId_Name",
                table: "Interfaces");

            migrationBuilder.DropIndex(
                name: "IX_Devices_Name",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_ChangeLog_Timestamp",
                table: "ChangeLog");
        }
    }
}
