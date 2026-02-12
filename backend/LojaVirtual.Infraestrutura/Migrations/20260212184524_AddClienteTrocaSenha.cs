using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaVirtual.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class AddClienteTrocaSenha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientesTrocaSenha",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    DataExpiracao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Utilizado = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientesTrocaSenha", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientesTrocaSenha_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientesTrocaSenha_ClienteId",
                table: "ClientesTrocaSenha",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesTrocaSenha_Codigo",
                table: "ClientesTrocaSenha",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientesTrocaSenha_Email_DataExpiracao",
                table: "ClientesTrocaSenha",
                columns: new[] { "Email", "DataExpiracao" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientesTrocaSenha");
        }
    }
}
