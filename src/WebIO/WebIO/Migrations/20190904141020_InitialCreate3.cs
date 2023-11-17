namespace Tpc.WebIO.Migrations
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class InitialCreate3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyValueEntity_Devices_DeviceEntityId",
                table: "PropertyValueEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_PropertyValueEntity_Interfaces_InterfaceEntityId",
                table: "PropertyValueEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_PropertyValueEntity_Streams_StreamEntityId",
                table: "PropertyValueEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PropertyValueEntity",
                table: "PropertyValueEntity");

            migrationBuilder.DropIndex(
                name: "IX_PropertyValueEntity_InterfaceEntityId",
                table: "PropertyValueEntity");

            migrationBuilder.DropIndex(
                name: "IX_PropertyValueEntity_StreamEntityId",
                table: "PropertyValueEntity");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "PropertyValueEntity");

            migrationBuilder.DropColumn(
                name: "InterfaceEntityId",
                table: "PropertyValueEntity");

            migrationBuilder.DropColumn(
                name: "StreamEntityId",
                table: "PropertyValueEntity");

            migrationBuilder.RenameTable(
                name: "PropertyValueEntity",
                newName: "DevicePropertyValueEntity");

            migrationBuilder.RenameIndex(
                name: "IX_PropertyValueEntity_DeviceEntityId",
                table: "DevicePropertyValueEntity",
                newName: "IX_DevicePropertyValueEntity_DeviceEntityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DevicePropertyValueEntity",
                table: "DevicePropertyValueEntity",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "InterfacePropertyValueEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Key = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true),
                    InterfaceEntityId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterfacePropertyValueEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterfacePropertyValueEntity_Interfaces_InterfaceEntityId",
                        column: x => x.InterfaceEntityId,
                        principalTable: "Interfaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StreamPropertyValueEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Key = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true),
                    StreamEntityId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamPropertyValueEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StreamPropertyValueEntity_Streams_StreamEntityId",
                        column: x => x.StreamEntityId,
                        principalTable: "Streams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterfacePropertyValueEntity_InterfaceEntityId",
                table: "InterfacePropertyValueEntity",
                column: "InterfaceEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_StreamPropertyValueEntity_StreamEntityId",
                table: "StreamPropertyValueEntity",
                column: "StreamEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_DevicePropertyValueEntity_Devices_DeviceEntityId",
                table: "DevicePropertyValueEntity",
                column: "DeviceEntityId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DevicePropertyValueEntity_Devices_DeviceEntityId",
                table: "DevicePropertyValueEntity");

            migrationBuilder.DropTable(
                name: "InterfacePropertyValueEntity");

            migrationBuilder.DropTable(
                name: "StreamPropertyValueEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DevicePropertyValueEntity",
                table: "DevicePropertyValueEntity");

            migrationBuilder.RenameTable(
                name: "DevicePropertyValueEntity",
                newName: "PropertyValueEntity");

            migrationBuilder.RenameIndex(
                name: "IX_DevicePropertyValueEntity_DeviceEntityId",
                table: "PropertyValueEntity",
                newName: "IX_PropertyValueEntity_DeviceEntityId");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "PropertyValueEntity",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "InterfaceEntityId",
                table: "PropertyValueEntity",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StreamEntityId",
                table: "PropertyValueEntity",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PropertyValueEntity",
                table: "PropertyValueEntity",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyValueEntity_InterfaceEntityId",
                table: "PropertyValueEntity",
                column: "InterfaceEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyValueEntity_StreamEntityId",
                table: "PropertyValueEntity",
                column: "StreamEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyValueEntity_Devices_DeviceEntityId",
                table: "PropertyValueEntity",
                column: "DeviceEntityId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyValueEntity_Interfaces_InterfaceEntityId",
                table: "PropertyValueEntity",
                column: "InterfaceEntityId",
                principalTable: "Interfaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyValueEntity_Streams_StreamEntityId",
                table: "PropertyValueEntity",
                column: "StreamEntityId",
                principalTable: "Streams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
