#nullable disable

namespace Tpc.WebIO.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class AddFulltextSearchIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                sql: "CREATE FULLTEXT CATALOG ftCatalog AS DEFAULT;",
                suppressTransaction: true);

            foreach (var (table, fields) in new []
                     {
                         ("\"Devices\"", "\"Name\",\"Comment\",\"DeviceType\",\"Creator\",\"Modifier\""),
                         ("\"Interfaces\"", "\"Name\",\"Comment\",\"Creator\",\"Modifier\""),
                         ("\"Streams\"", "\"Name\",\"Comment\",\"Creator\",\"Modifier\""),
                         ("\"DeviceProperties\"", "\"Value\""),
                         ("\"InterfaceProperties\"", "\"Value\""),
                         ("\"StreamProperties\"", "\"Value\""),
                     })
            {
                migrationBuilder.Sql(
                    sql: $"CREATE FULLTEXT INDEX ON {table}({fields}) KEY INDEX \"PK_{table.Trim('"')}\"",
                    suppressTransaction: true);
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var table in new []
                     {
                         "\"Devices\"",
                         "\"Interfaces\"",
                         "\"Streams\"",
                         "\"DeviceProperties\"",
                         "\"InterfaceProperties\"",
                         "\"StreamProperties\"",
                     })
            {
                migrationBuilder.Sql(
                    sql: $"DROP FULLTEXT INDEX ON {table}",
                    suppressTransaction: true);
            }

            migrationBuilder.Sql(
                sql: "DROP FULLTEXT CATALOG ftCatalog;",
                suppressTransaction: true);
        }
    }
}
