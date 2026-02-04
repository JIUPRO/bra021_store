using LojaVirtual.Infraestrutura.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaVirtual.Infraestrutura.Migrations
{
	[DbContext(typeof(LojaDbContext))]
	[Migration("20260203193000_AddMetodoPagamentoToPedido")]
	public partial class AddMetodoPagamentoToPedido : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
				 name: "MetodoPagamento",
				 table: "Pedidos",
				 type: "nvarchar(20)",
				 maxLength: 20,
				 nullable: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				 name: "MetodoPagamento",
				 table: "Pedidos");
		}

		protected override void BuildTargetModel(ModelBuilder modelBuilder)
		{
			// Mantido vazio para evitar falha de migração em runtime.
		}
	}
}
