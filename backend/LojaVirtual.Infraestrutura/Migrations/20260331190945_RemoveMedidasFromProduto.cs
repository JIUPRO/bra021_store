using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaVirtual.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMedidasFromProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Altura",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "Largura",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "Peso",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "Profundidade",
                table: "Produtos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Altura",
                table: "Produtos",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Largura",
                table: "Produtos",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Peso",
                table: "Produtos",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Profundidade",
                table: "Produtos",
                type: "float",
                nullable: true);
        }
    }
}
