using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolMgmt.Migrations
{
    /// <inheritdoc />
    public partial class FixTeacherClassRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teachers_ClassId",
                table: "Teachers");

            migrationBuilder.AddColumn<Guid>(
                name: "ClassTeacherId1",
                table: "Classes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_ClassId",
                table: "Teachers",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_ClassTeacherId1",
                table: "Classes",
                column: "ClassTeacherId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Teachers_ClassTeacherId1",
                table: "Classes",
                column: "ClassTeacherId1",
                principalTable: "Teachers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Teachers_ClassTeacherId1",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_ClassId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Classes_ClassTeacherId1",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "ClassTeacherId1",
                table: "Classes");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_ClassId",
                table: "Teachers",
                column: "ClassId",
                unique: true,
                filter: "[ClassId] IS NOT NULL");
        }
    }
}
