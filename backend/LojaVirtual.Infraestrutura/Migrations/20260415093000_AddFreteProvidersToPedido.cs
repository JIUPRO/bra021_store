using LojaVirtual.Infraestrutura.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaVirtual.Infraestrutura.Migrations
{
    [DbContext(typeof(LojaDbContext))]
    [Migration("20260415093000_AddFreteProvidersToPedido")]
    public partial class AddFreteProvidersToPedido : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProviderFreteUtilizado",
                table: "Pedidos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderLogisticaUtilizado",
                table: "Pedidos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProviderFreteUtilizado",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "ProviderLogisticaUtilizado",
                table: "Pedidos");
        }
    }
}
