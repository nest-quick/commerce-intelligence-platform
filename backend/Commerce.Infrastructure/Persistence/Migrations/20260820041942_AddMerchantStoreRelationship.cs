using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Commerce.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMerchantStoreRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Stores_MerchantId",
                table: "Stores",
                column: "MerchantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_Merchants_MerchantId",
                table: "Stores",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stores_Merchants_MerchantId",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Stores_MerchantId",
                table: "Stores");
        }
    }
}
