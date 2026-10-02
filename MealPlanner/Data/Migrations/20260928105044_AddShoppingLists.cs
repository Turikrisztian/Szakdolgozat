// A korábbi konzolnapló SQL-sémája alapján helyreállított migráció.
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddShoppingLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShoppingLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MealPlanId = table.Column<int>(type: "int", nullable: false),
                    GeneratedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShoppingLists_MealPlans_MealPlanId",
                        column: x => x.MealPlanId,
                        principalTable: "MealPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingListItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShoppingListId = table.Column<int>(type: "int", nullable: false),
                    IngredientId = table.Column<int>(type: "int", nullable: false),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    AvailableQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    IsPurchased = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingListItems", x => x.Id);
                    table.CheckConstraint("CK_ShoppingListItems_AvailableQuantity_NonNegative", "[AvailableQuantity] >= 0");
                    table.CheckConstraint("CK_ShoppingListItems_Quantity_Positive", "[Quantity] > 0");
                    table.CheckConstraint("CK_ShoppingListItems_RequiredQuantity_Positive", "[RequiredQuantity] > 0");
                    table.ForeignKey(
                        name: "FK_ShoppingListItems_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShoppingListItems_ShoppingLists_ShoppingListId",
                        column: x => x.ShoppingListId,
                        principalTable: "ShoppingLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShoppingListItems_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingListItems_IngredientId",
                table: "ShoppingListItems",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingListItems_ShoppingListId_IngredientId",
                table: "ShoppingListItems",
                columns: new[] { "ShoppingListId", "IngredientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingListItems_UnitId",
                table: "ShoppingListItems",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingLists_MealPlanId",
                table: "ShoppingLists",
                column: "MealPlanId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShoppingListItems");

            migrationBuilder.DropTable(
                name: "ShoppingLists");
        }
    }
}
