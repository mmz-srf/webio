namespace Tpc.WebIO.Migrations
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Comment = table.Column<string>(nullable: true),
                    Creator = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Modifier = table.Column<string>(nullable: true),
                    Modified = table.Column<DateTime>(nullable: false),
                    ModificationComment = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Interfaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DeviceId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Comment = table.Column<string>(nullable: true),
                    Creator = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Modifier = table.Column<string>(nullable: true),
                    Modified = table.Column<DateTime>(nullable: false),
                    ModificationComment = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interfaces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Streams",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    InterfaceId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Comment = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Direction = table.Column<int>(nullable: false),
                    Creator = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Modifier = table.Column<string>(nullable: true),
                    Modified = table.Column<DateTime>(nullable: false),
                    ModificationComment = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Streams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PropertyValueEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Key = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true),
                    DeviceEntityId = table.Column<Guid>(nullable: true),
                    InterfaceEntityId = table.Column<Guid>(nullable: true),
                    StreamEntityId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyValueEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyValueEntity_Devices_DeviceEntityId",
                        column: x => x.DeviceEntityId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyValueEntity_Interfaces_InterfaceEntityId",
                        column: x => x.InterfaceEntityId,
                        principalTable: "Interfaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyValueEntity_Streams_StreamEntityId",
                        column: x => x.StreamEntityId,
                        principalTable: "Streams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyValueEntity_DeviceEntityId",
                table: "PropertyValueEntity",
                column: "DeviceEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyValueEntity_InterfaceEntityId",
                table: "PropertyValueEntity",
                column: "InterfaceEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyValueEntity_StreamEntityId",
                table: "PropertyValueEntity",
                column: "StreamEntityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyValueEntity");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "Interfaces");

            migrationBuilder.DropTable(
                name: "Streams");
        }
    }
}
