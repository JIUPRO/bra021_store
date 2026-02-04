using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Enums;

namespace LojaVirtual.Dominio.Interfaces
{
    public interface IPedidoRepository : IBaseRepository<Pedido>
    {
        Task<Pedido?> ObterComItensAsync(Guid id);
        Task<IEnumerable<Pedido>> ObterPorClienteAsync(Guid clienteId);
        Task<IEnumerable<Pedido>> ObterPorStatusAsync(StatusPedido status);
        Task<IEnumerable<Pedido>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
        Task<string> GerarNumeroPedidoAsync();
    }
}
