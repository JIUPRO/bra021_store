using LojaVirtual.Infraestrutura.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaVirtual.Infraestrutura.Migrations
{
    [DbContext(typeof(LojaDbContext))]
    [Migration("20260401153118_RenameLogisticaIntegracaoFields")]
    public partial class RenameLogisticaIntegracaoFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('Pedidos', 'MelhorEnvioPedidoId') IS NOT NULL
   AND COL_LENGTH('Pedidos', 'IntegracaoFretePedidoId') IS NULL
BEGIN
    EXEC sp_rename N'[Pedidos].[MelhorEnvioPedidoId]', N'IntegracaoFretePedidoId', 'COLUMN';
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('Pedidos', 'MelhorEnvioProtocolo') IS NOT NULL
   AND COL_LENGTH('Pedidos', 'IntegracaoFreteProtocolo') IS NULL
BEGIN
    EXEC sp_rename N'[Pedidos].[MelhorEnvioProtocolo]', N'IntegracaoFreteProtocolo', 'COLUMN';
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('Pedidos', 'IntegracaoFretePedidoId') IS NOT NULL
   AND COL_LENGTH('Pedidos', 'MelhorEnvioPedidoId') IS NULL
BEGIN
    EXEC sp_rename N'[Pedidos].[IntegracaoFretePedidoId]', N'MelhorEnvioPedidoId', 'COLUMN';
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('Pedidos', 'IntegracaoFreteProtocolo') IS NOT NULL
   AND COL_LENGTH('Pedidos', 'MelhorEnvioProtocolo') IS NULL
BEGIN
    EXEC sp_rename N'[Pedidos].[IntegracaoFreteProtocolo]', N'MelhorEnvioProtocolo', 'COLUMN';
END
");
        }
    }
}
