using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using LojaVirtual.Dominio.Interfaces;
using LojaVirtual.Infraestrutura.Data;

namespace LojaVirtual.Infraestrutura.Repositories
{
	public class BaseRepository<T> : IBaseRepository<T> where T : class
	{
		protected readonly LojaDbContext _contexto;
		protected readonly DbSet<T> _dbSet;

		public BaseRepository(LojaDbContext contexto)
		{
			_contexto = contexto;
			_dbSet = contexto.Set<T>();
		}

		public virtual async Task<T?> ObterPorIdAsync(Guid id)
		{
			return await _dbSet.FindAsync(id);
		}

		public virtual async Task<IEnumerable<T>> ObterTodosAsync()
		{
			return await _dbSet.ToListAsync();
		}

		public virtual async Task<IEnumerable<T>> ObterPorFiltroAsync(Expression<Func<T, bool>> filtro)
		{
			return await _dbSet.Where(filtro).ToListAsync();
		}

		public virtual async Task<T> AdicionarAsync(T entidade)
		{
			await _dbSet.AddAsync(entidade);
			return entidade;
		}

		public virtual Task<T> AtualizarAsync(T entidade)
		{
			_dbSet.Update(entidade);
			return Task.FromResult(entidade);
		}

		public virtual async Task<bool> RemoverAsync(Guid id)
		{
			var entidade = await ObterPorIdAsync(id);
			if (entidade == null)
				return false;

			_dbSet.Remove(entidade);
			return true;
		}

		public virtual async Task<bool> ExisteAsync(Guid id)
		{
			return await _dbSet.FindAsync(id) != null;
		}

		public virtual async Task<int> SalvarMudancasAsync()
		{
			return await _contexto.SaveChangesAsync();
		}
	}
}
