using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseRegistrationHelper.Data.Migrations
{
    public partial class AddRolesSupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.RenameColumn(
            //    name: "Schedule",
            //    table: "Sections",
            //    newName: "Days");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.RenameColumn(
            //    name: "Days",
            //    table: "Sections",
            //    newName: "Schedule");
        }
    }
}
