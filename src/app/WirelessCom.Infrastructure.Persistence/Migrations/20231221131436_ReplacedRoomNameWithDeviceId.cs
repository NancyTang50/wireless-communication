using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WirelessCom.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplacedRoomNameWithDeviceId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RoomName",
                table: "RoomClimateReadings",
                newName: "DeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeviceId",
                table: "RoomClimateReadings",
                newName: "RoomName");
        }
    }
}
