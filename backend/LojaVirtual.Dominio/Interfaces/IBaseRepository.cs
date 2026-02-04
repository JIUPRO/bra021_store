using System.Linq.Expressions;

namespace LojaVirtual.Dominio.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T?> ObterPorIdAsync(Guid id);
        Task<IEnumerable<T>> ObterTodosAsync();
        Task<IEnumerable<T>> ObterPorFiltroAsync(Expression<Func<T, bool>> filtro);
        Task<T> AdicionarAsync(T entidade);
        Task<T> AtualizarAsync(T entidade);
        Task<bool> RemoverAsync(Guid id);
        Task<bool> ExisteAsync(Guid id);
        Task<int> SalvarMudancasAsync();
    }
}
