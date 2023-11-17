namespace Tpc.WebIO.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class InitialCreate4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DevicePropertyValueEntity_Devices_DeviceEntityId",
                table: "DevicePropertyValueEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_InterfacePropertyValueEntity_Interfaces_InterfaceEntityId",
                table: "InterfacePropertyValueEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_StreamPropertyValueEntity_Streams_StreamEntityId",
                table: "StreamPropertyValueEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StreamPropertyValueEntity",
                table: "StreamPropertyValueEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InterfacePropertyValueEntity",
                table: "InterfacePropertyValueEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DevicePropertyValueEntity",
                table: "DevicePropertyValueEntity");

            migrationBuilder.RenameTable(
                name: "StreamPropertyValueEntity",
                newName: "StreamProperties");

            migrationBuilder.RenameTable(
                name: "InterfacePropertyValueEntity",
                newName: "InterfaceProperties");

            migrationBuilder.RenameTable(
                name: "DevicePropertyValueEntity",
                newName: "DeviceProperties");

            migrationBuilder.RenameIndex(
                name: "IX_StreamPropertyValueEntity_StreamEntityId",
                table: "StreamProperties",
                newName: "IX_StreamProperties_StreamEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_InterfacePropertyValueEntity_InterfaceEntityId",
                table: "InterfaceProperties",
                newName: "IX_InterfaceProperties_InterfaceEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_DevicePropertyValueEntity_DeviceEntityId",
                table: "DeviceProperties",
                newName: "IX_DeviceProperties_DeviceEntityId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Streams",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Modifier",
                table: "Streams",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Creator",
                table: "Streams",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Interfaces",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Modifier",
                table: "Interfaces",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Creator",
                table: "Interfaces",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Devices",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Modifier",
                table: "Devices",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Creator",
                table: "Devices",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "StreamProperties",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "StreamProperties",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "InterfaceProperties",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "InterfaceProperties",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "DeviceProperties",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "DeviceProperties",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_StreamProperties",
                table: "StreamProperties",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InterfaceProperties",
                table: "InterfaceProperties",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceProperties",
                table: "DeviceProperties",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceProperties_Devices_DeviceEntityId",
                table: "DeviceProperties",
                column: "DeviceEntityId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceProperties_Devices_DeviceEntityId",
                table: "DeviceProperties");

            migrationBuilder.DropForeignKey(
                name: "FK_InterfaceProperties_Interfaces_InterfaceEntityId",
                table: "InterfaceProperties");

            migrationBuilder.DropForeignKey(
                name: "FK_StreamProperties_Streams_StreamEntityId",
                table: "StreamProperties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StreamProperties",
                table: "StreamProperties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InterfaceProperties",
                table: "InterfaceProperties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceProperties",
                table: "DeviceProperties");

            migrationBuilder.RenameTable(
                name: "StreamProperties",
                newName: "StreamPropertyValueEntity");

            migrationBuilder.RenameTable(
                name: "InterfaceProperties",
                newName: "InterfacePropertyValueEntity");

            migrationBuilder.RenameTable(
                name: "DeviceProperties",
                newName: "DevicePropertyValueEntity");

            migrationBuilder.RenameIndex(
                name: "IX_StreamProperties_StreamEntityId",
                table: "StreamPropertyValueEntity",
                newName: "IX_StreamPropertyValueEntity_StreamEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_InterfaceProperties_InterfaceEntityId",
                table: "InterfacePropertyValueEntity",
                newName: "IX_InterfacePropertyValueEntity_InterfaceEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceProperties_DeviceEntityId",
                table: "DevicePropertyValueEntity",
                newName: "IX_DevicePropertyValueEntity_DeviceEntityId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Streams",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Modifier",
                table: "Streams",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Creator",
                table: "Streams",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Interfaces",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Modifier",
                table: "Interfaces",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Creator",
                table: "Interfaces",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Devices",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Modifier",
                table: "Devices",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Creator",
                table: "Devices",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "StreamPropertyValueEntity",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "StreamPropertyValueEntity",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "InterfacePropertyValueEntity",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "InterfacePropertyValueEntity",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "DevicePropertyValueEntity",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "DevicePropertyValueEntity",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_StreamPropertyValueEntity",
                table: "StreamPropertyValueEntity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InterfacePropertyValueEntity",
                table: "InterfacePropertyValueEntity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DevicePropertyValueEntity",
                table: "DevicePropertyValueEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DevicePropertyValueEntity_Devices_DeviceEntityId",
                table: "DevicePropertyValueEntity",
                column: "DeviceEntityId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InterfacePropertyValueEntity_Interfaces_InterfaceEntityId",
                table: "InterfacePropertyValueEntity",
                column: "InterfaceEntityId",
                principalTable: "Interfaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StreamPropertyValueEntity_Streams_StreamEntityId",
                table: "StreamPropertyValueEntity",
                column: "StreamEntityId",
                principalTable: "Streams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
