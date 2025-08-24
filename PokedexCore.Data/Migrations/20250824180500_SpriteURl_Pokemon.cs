using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokedexCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class SpriteURl_Pokemon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SpriteURL",
                table: "Pokemons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpriteURL",
                table: "Pokemons");
        }
    }
}
