using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokedexCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class CAMBIOSTrainer : Migration
    {
        /// <inheritdoc />

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Elimina las FK existentes
            migrationBuilder.DropForeignKey(
                name: "FK_TrainerPokemons_Trainers_TrainerId",
                table: "TrainerPokemons");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainerPokemons_Trainers_TrainerId1",
                table: "TrainerPokemons");

            // Elimina el índice y la columna duplicada
            migrationBuilder.DropIndex(
                name: "IX_TrainerPokemons_TrainerId1",
                table: "TrainerPokemons");

            migrationBuilder.DropColumn(
                name: "TrainerId1",
                table: "TrainerPokemons");

            // Agrega la FK principal sin cascada
            migrationBuilder.AddForeignKey(
                name: "FK_TrainerPokemons_Trainers_TrainerId",
                table: "TrainerPokemons",
                column: "TrainerId",
                principalTable: "Trainers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Elimina la FK actual
            migrationBuilder.DropForeignKey(
                name: "FK_TrainerPokemons_Trainers_TrainerId",
                table: "TrainerPokemons");

            // Vuelve a crear la columna duplicada
            migrationBuilder.AddColumn<int>(
                name: "TrainerId1",
                table: "TrainerPokemons",
                type: "int",
                nullable: true);

            // Vuelve a crear el índice para TrainerId1
            migrationBuilder.CreateIndex(
                name: "IX_TrainerPokemons_TrainerId1",
                table: "TrainerPokemons",
                column: "TrainerId1");

            // Agrega las FK anteriores con cascada (si así estaban antes)
            migrationBuilder.AddForeignKey(
                name: "FK_TrainerPokemons_Trainers_TrainerId",
                table: "TrainerPokemons",
                column: "TrainerId",
                principalTable: "Trainers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainerPokemons_Trainers_TrainerId1",
                table: "TrainerPokemons",
                column: "TrainerId1",
                principalTable: "Trainers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

    }
}
