using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QM_AI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPrePostWaitTimeToRoutingStep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "pre_wait_time_minutes",
                table: "routing_steps",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "post_wait_time_minutes",
                table: "routing_steps",
                type: "double",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "pre_wait_time_minutes",
                table: "routing_steps");

            migrationBuilder.DropColumn(
                name: "post_wait_time_minutes",
                table: "routing_steps");
        }
    }
}
