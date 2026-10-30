using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceToClassBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Attended",
                table: "ClassBookings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attended",
                table: "ClassBookings");
        }
    }
}
