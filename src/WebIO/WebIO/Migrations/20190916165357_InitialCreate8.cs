namespace Tpc.WebIO.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class InitialCreate8 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InterfaceProperties_Interfaces_InterfaceEntityId",
                table: "InterfaceProperties");

            migrationBuilder.DropForeignKey(
                name: "FK_StreamProperties_Streams_StreamEntityId",
                table: "StreamProperties");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Streams",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "StreamProperties",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "StreamEntityId",
                table: "StreamProperties",
                newName: "StreamId");

            migrationBuilder.RenameIndex(
                name: "IX_StreamProperties_StreamEntityId",
                table: "StreamProperties",
                newName: "IX_StreamProperties_StreamId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Interfaces",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "InterfaceProperties",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "InterfaceEntityId",
                table: "InterfaceProperties",
                newName: "InterfaceId");

            migrationBuilder.RenameIndex(
                name: "IX_InterfaceProperties_InterfaceEntityId",
                table: "InterfaceProperties",
                newName: "IX_InterfaceProperties_InterfaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_InterfaceProperties_Interfaces_InterfaceId",
                table: "InterfaceProperties",
                column: "InterfaceId",
                principalTable: "Interfaces",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StreamProperties_Streams_StreamId",
                table: "StreamProperties",
                column: "StreamId",
                principalTable: "Streams",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InterfaceProperties_Interfaces_InterfaceId",
                table: "InterfaceProperties");

            migrationBuilder.DropForeignKey(
                name: "FK_StreamProperties_Streams_StreamId",
                table: "StreamProperties");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Streams",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "StreamProperties",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "StreamId",
                table: "StreamProperties",
                newName: "StreamEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_StreamProperties_StreamId",
                table: "StreamProperties",
                newName: "IX_StreamProperties_StreamEntityId");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Interfaces",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "InterfaceProperties",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "InterfaceId",
                table: "InterfaceProperties",
                newName: "InterfaceEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_InterfaceProperties_InterfaceId",
                table: "InterfaceProperties",
                newName: "IX_InterfaceProperties_InterfaceEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_InterfaceProperties_Interfaces_InterfaceEntityId",
                table: "InterfaceProperties",
                column: "InterfaceEntityId",
                principalTable: "Interfaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StreamProperties_Streams_StreamEntityId",
                table: "StreamProperties",
                column: "StreamEntityId",
                principalTable: "Streams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
