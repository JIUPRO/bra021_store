using LojaVirtual.Dominio.Entidades;

namespace LojaVirtual.Dominio.Interfaces
{
    public interface ICategoriaRepository : IBaseRepository<Categoria>
    {
        Task<Categoria?> ObterComProdutosAsync(Guid id);
    }
}
