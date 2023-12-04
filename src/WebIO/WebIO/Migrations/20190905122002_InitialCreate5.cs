namespace Tpc.WebIO.Migrations
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class InitialCreate5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DevicesDenormalized",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    InterfaceCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicesDenormalized", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InterfacesDenormalized",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    StreamsCountVideoSend = table.Column<int>(nullable: false),
                    StreamsCountAudioSend = table.Column<int>(nullable: false),
                    StreamsCountAncillarySend = table.Column<int>(nullable: false),
                    StreamsCountVideoReceive = table.Column<int>(nullable: false),
                    StreamsCountAudioReceive = table.Column<int>(nullable: false),
                    StreamsCountAncillaryReceive = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterfacesDenormalized", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DevicesDenormalized");

            migrationBuilder.DropTable(
                name: "InterfacesDenormalized");
        }
    }
}
