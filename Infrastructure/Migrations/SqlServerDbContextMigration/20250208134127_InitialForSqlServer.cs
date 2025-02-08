using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations.SqlServerDbContextMigration
{
    /// <inheritdoc />
    public partial class InitialForSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerID);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    LocationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerID = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.LocationID);
                    table.ForeignKey(
                        name: "FK_Locations_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "CustomerID", "Email", "Name", "Phone" },
                values: new object[,]
                {
                    { 1, "john.doe@example.com", "John Doe", "123-456-7890" },
                    { 2, "jane.smith@example.com", "Jane Smith", "234-567-8901" },
                    { 3, "michael.johnson@example.com", "Michael Johnson", "345-678-9012" },
                    { 4, "emily.davis@example.com", "Emily Davis", "456-789-0123" },
                    { 5, "robert.wilson@example.com", "Robert Wilson", "567-890-1234" },
                    { 6, "jessica.brown@example.com", "Jessica Brown", "678-901-2345" },
                    { 7, "david.miller@example.com", "David Miller", "789-012-3456" },
                    { 8, "sarah.taylor@example.com", "Sarah Taylor", "890-123-4567" },
                    { 9, "christopher.martinez@example.com", "Christopher Martinez", "901-234-5678" },
                    { 10, "amanda.anderson@example.com", "Amanda Anderson", "012-345-6789" }
                });

            migrationBuilder.InsertData(
                table: "Locations",
                columns: new[] { "LocationID", "Address", "CustomerID" },
                values: new object[,]
                {
                    { 1, "123 Main St, City A", 1 },
                    { 2, "456 Oak St, City A", 1 },
                    { 3, "789 Pine St, City B", 2 },
                    { 4, "101 Maple St, City B", 2 },
                    { 5, "202 Cedar St, City C", 3 },
                    { 6, "303 Birch St, City C", 3 },
                    { 7, "404 Walnut St, City D", 4 },
                    { 8, "505 Chestnut St, City D", 4 },
                    { 9, "606 Spruce St, City E", 5 },
                    { 10, "707 Redwood St, City E", 5 },
                    { 11, "808 Elm St, City F", 6 },
                    { 12, "909 Cypress St, City F", 6 },
                    { 13, "1010 Fir St, City G", 7 },
                    { 14, "1111 Beech St, City G", 7 },
                    { 15, "1212 Dogwood St, City H", 8 },
                    { 16, "1313 Magnolia St, City H", 8 },
                    { 17, "1414 Aspen St, City I", 9 },
                    { 18, "1515 Willow St, City I", 9 },
                    { 19, "1616 Hickory St, City J", 10 },
                    { 20, "1717 Sycamore St, City J", 10 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_CustomerID",
                table: "Locations",
                column: "CustomerID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
