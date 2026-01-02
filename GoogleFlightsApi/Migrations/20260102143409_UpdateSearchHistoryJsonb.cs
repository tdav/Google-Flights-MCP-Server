using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GoogleFlightsApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSearchHistoryJsonb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlightResults");

            migrationBuilder.AddColumn<string>(
                name: "FlightsJson",
                table: "SearchHistories",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlightsJson",
                table: "SearchHistories");

            migrationBuilder.CreateTable(
                name: "FlightResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SearchHistoryId = table.Column<int>(type: "integer", nullable: false),
                    Airline = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ArrivalTime = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DepartureTime = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Duration = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FlightNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Stops = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlightResults_SearchHistories_SearchHistoryId",
                        column: x => x.SearchHistoryId,
                        principalTable: "SearchHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlightResults_SearchHistoryId",
                table: "FlightResults",
                column: "SearchHistoryId");
        }
    }
}
