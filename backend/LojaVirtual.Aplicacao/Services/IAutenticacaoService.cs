using LojaVirtual.Aplicacao.DTOs;

namespace LojaVirtual.Aplicacao.Services
{
    public interface IAutenticacaoService
    {
        Task<(bool sucesso, string mensagem, ClienteDTO? cliente)> AutenticarAsync(LoginClienteDTO dto);
        Task<(bool sucesso, string mensagem)> EsqueceuSenhaAsync(EsqueceuSenhaDTO dto);
        Task<(bool sucesso, string mensagem)> ResetarSenhaAsync(ResetarSenhaDTO dto);
    }
}
