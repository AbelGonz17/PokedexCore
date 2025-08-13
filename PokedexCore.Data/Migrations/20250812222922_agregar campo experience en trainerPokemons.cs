using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokedexCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class agregarcampoexperienceentrainerPokemons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Experience",
                table: "TrainerPokemons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TrainerId1",
                table: "TrainerPokemons",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainerPokemons_PokemonId",
                table: "TrainerPokemons",
                column: "PokemonId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainerPokemons_TrainerId",
                table: "TrainerPokemons",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainerPokemons_TrainerId1",
                table: "TrainerPokemons",
                column: "TrainerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainerPokemons_Pokemons_PokemonId",
                table: "TrainerPokemons",
                column: "PokemonId",
                principalTable: "Pokemons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainerPokemons_Trainers_TrainerId",
                table: "TrainerPokemons",
                column: "TrainerId",
                principalTable: "Trainers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainerPokemons_Trainers_TrainerId1",
                table: "TrainerPokemons",
                column: "TrainerId1",
                principalTable: "Trainers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainerPokemons_Pokemons_PokemonId",
                table: "TrainerPokemons");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainerPokemons_Trainers_TrainerId",
                table: "TrainerPokemons");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainerPokemons_Trainers_TrainerId1",
                table: "TrainerPokemons");

            migrationBuilder.DropIndex(
                name: "IX_TrainerPokemons_PokemonId",
                table: "TrainerPokemons");

            migrationBuilder.DropIndex(
                name: "IX_TrainerPokemons_TrainerId",
                table: "TrainerPokemons");

            migrationBuilder.DropIndex(
                name: "IX_TrainerPokemons_TrainerId1",
                table: "TrainerPokemons");

            migrationBuilder.DropColumn(
                name: "Experience",
                table: "TrainerPokemons");

            migrationBuilder.DropColumn(
                name: "TrainerId1",
                table: "TrainerPokemons");
        }
    }
}
