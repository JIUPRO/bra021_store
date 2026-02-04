using LojaVirtual.Dominio.Entidades;

namespace LojaVirtual.Dominio.Interfaces
{
    public interface IClienteRepository : IBaseRepository<Cliente>
    {
        Task<Cliente?> ObterPorEmailAsync(string email);
        Task<Cliente?> ObterPorCpfAsync(string cpf);
        Task<IEnumerable<Cliente>> ObterClientesComPedidosAsync();
    }
}
