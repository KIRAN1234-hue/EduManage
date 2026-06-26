using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolMgmt.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Submissions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Feedback",
                table: "Submissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MarksAwarded",
                table: "Submissions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AppliedOn",
                table: "LeaveApplications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "LeaveApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedById",
                table: "LeaveApplications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeaveType",
                table: "LeaveApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "LeaveApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "LeaveApplications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "TotalMarks",
                table: "Assignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplications_ApprovedById",
                table: "LeaveApplications",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplications_UserId",
                table: "LeaveApplications",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveApplications_AspNetUsers_ApprovedById",
                table: "LeaveApplications",
                column: "ApprovedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveApplications_AspNetUsers_UserId",
                table: "LeaveApplications",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveApplications_AspNetUsers_ApprovedById",
                table: "LeaveApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveApplications_AspNetUsers_UserId",
                table: "LeaveApplications");

            migrationBuilder.DropIndex(
                name: "IX_LeaveApplications_ApprovedById",
                table: "LeaveApplications");

            migrationBuilder.DropIndex(
                name: "IX_LeaveApplications_UserId",
                table: "LeaveApplications");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "Feedback",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "MarksAwarded",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "AppliedOn",
                table: "LeaveApplications");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "LeaveApplications");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "LeaveApplications");

            migrationBuilder.DropColumn(
                name: "LeaveType",
                table: "LeaveApplications");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "LeaveApplications");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "LeaveApplications");

            migrationBuilder.DropColumn(
                name: "TotalMarks",
                table: "Assignments");
        }
    }
}
