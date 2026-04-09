using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaVirtual.Infraestrutura.Migrations
{
    public partial class RenameLogisticaIntegracaoFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MelhorEnvioPedidoId",
                table: "Pedidos",
                newName: "IntegracaoFretePedidoId");

            migrationBuilder.RenameColumn(
                name: "MelhorEnvioProtocolo",
                table: "Pedidos",
                newName: "IntegracaoFreteProtocolo");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IntegracaoFretePedidoId",
                table: "Pedidos",
                newName: "MelhorEnvioPedidoId");

            migrationBuilder.RenameColumn(
                name: "IntegracaoFreteProtocolo",
                table: "Pedidos",
                newName: "MelhorEnvioProtocolo");
        }
    }
}
