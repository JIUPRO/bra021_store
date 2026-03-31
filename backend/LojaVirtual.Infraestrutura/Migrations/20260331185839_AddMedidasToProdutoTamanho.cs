using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaVirtual.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class AddMedidasToProdutoTamanho : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Altura",
                table: "ProdutoTamanhos",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Largura",
                table: "ProdutoTamanhos",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Peso",
                table: "ProdutoTamanhos",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Profundidade",
                table: "ProdutoTamanhos",
                type: "float",
                nullable: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Altura",
                table: "ProdutoTamanhos");

            migrationBuilder.DropColumn(
                name: "Largura",
                table: "ProdutoTamanhos");

            migrationBuilder.DropColumn(
                name: "Peso",
                table: "ProdutoTamanhos");

            migrationBuilder.DropColumn(
                name: "Profundidade",
                table: "ProdutoTamanhos");
        }
    }
}
