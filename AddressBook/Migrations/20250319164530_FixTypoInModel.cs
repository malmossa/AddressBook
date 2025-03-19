using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressBook.Migrations
{
    /// <inheritdoc />
    public partial class FixTypoInModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_AspNetUsers_AppUserId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_AspNetUsers_AppUserId",
                table: "Contacts");

            migrationBuilder.RenameColumn(
                name: "state",
                table: "Contacts",
                newName: "State");

            migrationBuilder.RenameColumn(
                name: "AppUserId",
                table: "Contacts",
                newName: "AppUserID");

            migrationBuilder.RenameColumn(
                name: "Birthday",
                table: "Contacts",
                newName: "BirthDate");

            migrationBuilder.RenameIndex(
                name: "IX_Contacts_AppUserId",
                table: "Contacts",
                newName: "IX_Contacts_AppUserID");

            migrationBuilder.RenameColumn(
                name: "AppUserId",
                table: "Categories",
                newName: "AppUserID");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_AppUserId",
                table: "Categories",
                newName: "IX_Categories_AppUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_AspNetUsers_AppUserID",
                table: "Categories",
                column: "AppUserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_AspNetUsers_AppUserID",
                table: "Contacts",
                column: "AppUserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_AspNetUsers_AppUserID",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_AspNetUsers_AppUserID",
                table: "Contacts");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "Contacts",
                newName: "state");

            migrationBuilder.RenameColumn(
                name: "AppUserID",
                table: "Contacts",
                newName: "AppUserId");

            migrationBuilder.RenameColumn(
                name: "BirthDate",
                table: "Contacts",
                newName: "Birthday");

            migrationBuilder.RenameIndex(
                name: "IX_Contacts_AppUserID",
                table: "Contacts",
                newName: "IX_Contacts_AppUserId");

            migrationBuilder.RenameColumn(
                name: "AppUserID",
                table: "Categories",
                newName: "AppUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_AppUserID",
                table: "Categories",
                newName: "IX_Categories_AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_AspNetUsers_AppUserId",
                table: "Categories",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_AspNetUsers_AppUserId",
                table: "Contacts",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
