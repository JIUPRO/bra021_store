using LojaVirtual.Dominio.Entidades;

namespace LojaVirtual.Dominio.Interfaces
{
    public interface IUsuarioTrocaSenhaRepository : IBaseRepository<UsuarioTrocaSenha>
    {
        Task<UsuarioTrocaSenha?> ObterPorCodigoAsync(string codigo);
        Task<IEnumerable<UsuarioTrocaSenha>> ObterPorUsuarioIdAsync(Guid usuarioId);
        Task<UsuarioTrocaSenha?> ObterPorEmailECodigoAsync(string email, string codigo);
        Task<bool> LimparExpiradosAsync();
    }
}
