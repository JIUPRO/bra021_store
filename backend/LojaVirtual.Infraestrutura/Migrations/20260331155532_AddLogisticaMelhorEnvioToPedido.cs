using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaVirtual.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class AddLogisticaMelhorEnvioToPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoRastreio",
                table: "Pedidos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataEntrega",
                table: "Pedidos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataEtiquetaGerada",
                table: "Pedidos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataPostagem",
                table: "Pedidos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MelhorEnvioPedidoId",
                table: "Pedidos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MelhorEnvioProtocolo",
                table: "Pedidos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusLogistico",
                table: "Pedidos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlEtiqueta",
                table: "Pedidos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlRastreio",
                table: "Pedidos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoRastreio",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "DataEntrega",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "DataEtiquetaGerada",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "DataPostagem",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "MelhorEnvioPedidoId",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "MelhorEnvioProtocolo",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "StatusLogistico",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "UrlEtiqueta",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "UrlRastreio",
                table: "Pedidos");
        }
    }
}
