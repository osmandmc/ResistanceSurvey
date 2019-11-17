using Microsoft.EntityFrameworkCore.Migrations;

namespace RS.EF.Data.Migrations
{
    public partial class descriptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
       
          
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Resistance");

          

        }
    }
}
