using LojaVirtual.Infraestrutura.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaVirtual.Infraestrutura.Migrations
{
    [DbContext(typeof(LojaDbContext))]
    [Migration("20260401155544_RenameParametrosLogistica")]
    public partial class RenameParametrosLogistica : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE ParametrosSistema SET Chave = 'LogisticaNaoComercial', Secao = 'Logística' WHERE Chave = 'MelhorEnvioNaoComercial';
UPDATE ParametrosSistema SET Chave = 'LogisticaRemetenteNome', Secao = 'Logística' WHERE Chave = 'MelhorEnvioRemetenteNome';
UPDATE ParametrosSistema SET Chave = 'LogisticaRemetenteTelefone', Secao = 'Logística' WHERE Chave = 'MelhorEnvioRemetenteTelefone';
UPDATE ParametrosSistema SET Chave = 'LogisticaRemetenteEmail', Secao = 'Logística' WHERE Chave = 'MelhorEnvioRemetenteEmail';
UPDATE ParametrosSistema SET Chave = 'LogisticaRemetenteDocumento', Secao = 'Logística' WHERE Chave = 'MelhorEnvioRemetenteDocumento';
UPDATE ParametrosSistema SET Chave = 'LogisticaRemetenteInscricaoEstadual', Secao = 'Logística' WHERE Chave = 'MelhorEnvioRemetenteInscricaoEstadual';
UPDATE ParametrosSistema SET Chave = 'LogisticaRemetenteLogradouro', Secao = 'Logística' WHERE Chave = 'MelhorEnvioRemetenteLogradouro';
UPDATE ParametrosSistema SET Chave = 'LogisticaRemetenteNumero', Secao = 'Logística' WHERE Chave = 'MelhorEnvioRemetenteNumero';
UPDATE ParametrosSistema SET Chave = 'LogisticaRemetenteComplemento', Secao = 'Logística' WHERE Chave = 'MelhorEnvioRemetenteComplemento';
UPDATE ParametrosSistema SET Chave = 'LogisticaRemetenteBairro', Secao = 'Logística' WHERE Chave = 'MelhorEnvioRemetenteBairro';
UPDATE ParametrosSistema SET Chave = 'LogisticaRemetenteCidade', Secao = 'Logística' WHERE Chave = 'MelhorEnvioRemetenteCidade';
UPDATE ParametrosSistema SET Chave = 'LogisticaRemetenteEstado', Secao = 'Logística' WHERE Chave = 'MelhorEnvioRemetenteEstado';
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE ParametrosSistema SET Chave = 'MelhorEnvioNaoComercial', Secao = 'Logística' WHERE Chave = 'LogisticaNaoComercial';
UPDATE ParametrosSistema SET Chave = 'MelhorEnvioRemetenteNome', Secao = 'Logística' WHERE Chave = 'LogisticaRemetenteNome';
UPDATE ParametrosSistema SET Chave = 'MelhorEnvioRemetenteTelefone', Secao = 'Logística' WHERE Chave = 'LogisticaRemetenteTelefone';
UPDATE ParametrosSistema SET Chave = 'MelhorEnvioRemetenteEmail', Secao = 'Logística' WHERE Chave = 'LogisticaRemetenteEmail';
UPDATE ParametrosSistema SET Chave = 'MelhorEnvioRemetenteDocumento', Secao = 'Logística' WHERE Chave = 'LogisticaRemetenteDocumento';
UPDATE ParametrosSistema SET Chave = 'MelhorEnvioRemetenteInscricaoEstadual', Secao = 'Logística' WHERE Chave = 'LogisticaRemetenteInscricaoEstadual';
UPDATE ParametrosSistema SET Chave = 'MelhorEnvioRemetenteLogradouro', Secao = 'Logística' WHERE Chave = 'LogisticaRemetenteLogradouro';
UPDATE ParametrosSistema SET Chave = 'MelhorEnvioRemetenteNumero', Secao = 'Logística' WHERE Chave = 'LogisticaRemetenteNumero';
UPDATE ParametrosSistema SET Chave = 'MelhorEnvioRemetenteComplemento', Secao = 'Logística' WHERE Chave = 'LogisticaRemetenteComplemento';
UPDATE ParametrosSistema SET Chave = 'MelhorEnvioRemetenteBairro', Secao = 'Logística' WHERE Chave = 'LogisticaRemetenteBairro';
UPDATE ParametrosSistema SET Chave = 'MelhorEnvioRemetenteCidade', Secao = 'Logística' WHERE Chave = 'LogisticaRemetenteCidade';
UPDATE ParametrosSistema SET Chave = 'MelhorEnvioRemetenteEstado', Secao = 'Logística' WHERE Chave = 'LogisticaRemetenteEstado';
");
        }
    }
}
