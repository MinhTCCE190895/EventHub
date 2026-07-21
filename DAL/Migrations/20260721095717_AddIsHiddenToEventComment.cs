using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddIsHiddenToEventComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventCategories_Categories_CategoryId",
                table: "EventCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_EventTags_Tags_TagId",
                table: "EventTags");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_StudentId",
                table: "Bookings");

            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "EventComments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_StudentId_EventId",
                table: "Bookings",
                columns: new[] { "StudentId", "EventId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EventCategories_Categories_CategoryId",
                table: "EventCategories",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventTags_Tags_TagId",
                table: "EventTags",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventCategories_Categories_CategoryId",
                table: "EventCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_EventTags_Tags_TagId",
                table: "EventTags");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_StudentId_EventId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "EventComments");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_StudentId",
                table: "Bookings",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventCategories_Categories_CategoryId",
                table: "EventCategories",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventTags_Tags_TagId",
                table: "EventTags",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
