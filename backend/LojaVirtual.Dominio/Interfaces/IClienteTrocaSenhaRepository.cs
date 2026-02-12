using LojaVirtual.Dominio.Entidades;

namespace LojaVirtual.Dominio.Interfaces
{
    public interface IClienteTrocaSenhaRepository : IBaseRepository<ClienteTrocaSenha>
    {
        Task<ClienteTrocaSenha?> ObterPorCodigoAsync(string codigo);
        Task<IEnumerable<ClienteTrocaSenha>> ObterPorClienteIdAsync(Guid clienteId);
        Task<ClienteTrocaSenha?> ObterPorEmailECodigoAsync(string email, string codigo);
        Task<bool> LimparExpiradosAsync();
    }
}
