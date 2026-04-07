using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordDuel.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomCodeToMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RoomCode",
                table: "Matches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoomCode",
                table: "Matches");
        }
    }
}
