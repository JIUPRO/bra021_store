using LojaVirtual.Dominio.Entidades;

namespace LojaVirtual.Dominio.Interfaces
{
    public interface IProdutoRepository : IBaseRepository<Produto>
    {
        Task<IEnumerable<Produto>> ObterProdutosEmDestaqueAsync();
        Task<IEnumerable<Produto>> ObterPorCategoriaAsync(Guid categoriaId);
        Task<IEnumerable<Produto>> ObterComEstoqueBaixoAsync();
        Task<IEnumerable<Produto>> PesquisarAsync(string termo);
    }
}
