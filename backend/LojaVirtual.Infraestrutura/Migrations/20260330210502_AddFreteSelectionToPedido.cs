using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaVirtual.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class AddFreteSelectionToPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoServicoFrete",
                table: "Pedidos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServicoFrete",
                table: "Pedidos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoEntrega",
                table: "Pedidos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransportadoraFrete",
                table: "Pedidos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoServicoFrete",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "ServicoFrete",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "TipoEntrega",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "TransportadoraFrete",
                table: "Pedidos");
        }
    }
}
