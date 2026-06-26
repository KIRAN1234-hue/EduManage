using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolMgmt.Migrations
{
    /// <inheritdoc />
    public partial class Week8FinalConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedAt",
                table: "LibraryBooks");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "LibraryBooks");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "FeePayments");

            migrationBuilder.RenameColumn(
                name: "Genre",
                table: "LibraryBooks",
                newName: "Publisher");

            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "FeePayments",
                newName: "Remarks");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "LibraryBooks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "LibraryBooks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PublishedYear",
                table: "LibraryBooks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ShelfLocation",
                table: "LibraryBooks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClassId1",
                table: "FeeStructures",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "FeeStructures",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "FeePayments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "FeePayments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReceiptNumber",
                table: "FeePayments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordedByUserId",
                table: "FeePayments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "BookIssues",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "BookIssues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_FeeStructures_ClassId1",
                table: "FeeStructures",
                column: "ClassId1");

            migrationBuilder.CreateIndex(
                name: "IX_FeePayments_RecordedByUserId",
                table: "FeePayments",
                column: "RecordedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FeePayments_AspNetUsers_RecordedByUserId",
                table: "FeePayments",
                column: "RecordedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FeeStructures_Classes_ClassId1",
                table: "FeeStructures",
                column: "ClassId1",
                principalTable: "Classes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeePayments_AspNetUsers_RecordedByUserId",
                table: "FeePayments");

            migrationBuilder.DropForeignKey(
                name: "FK_FeeStructures_Classes_ClassId1",
                table: "FeeStructures");

            migrationBuilder.DropIndex(
                name: "IX_FeeStructures_ClassId1",
                table: "FeeStructures");

            migrationBuilder.DropIndex(
                name: "IX_FeePayments_RecordedByUserId",
                table: "FeePayments");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "LibraryBooks");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "LibraryBooks");

            migrationBuilder.DropColumn(
                name: "PublishedYear",
                table: "LibraryBooks");

            migrationBuilder.DropColumn(
                name: "ShelfLocation",
                table: "LibraryBooks");

            migrationBuilder.DropColumn(
                name: "ClassId1",
                table: "FeeStructures");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "FeeStructures");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "FeePayments");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "FeePayments");

            migrationBuilder.DropColumn(
                name: "ReceiptNumber",
                table: "FeePayments");

            migrationBuilder.DropColumn(
                name: "RecordedByUserId",
                table: "FeePayments");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "BookIssues");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "BookIssues");

            migrationBuilder.RenameColumn(
                name: "Publisher",
                table: "LibraryBooks",
                newName: "Genre");

            migrationBuilder.RenameColumn(
                name: "Remarks",
                table: "FeePayments",
                newName: "TransactionId");

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedAt",
                table: "LibraryBooks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "LibraryBooks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "FeePayments",
                type: "datetime2",
                nullable: true);
        }
    }
}
