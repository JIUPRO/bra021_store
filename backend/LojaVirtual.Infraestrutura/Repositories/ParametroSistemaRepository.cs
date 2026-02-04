using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Interfaces;
using LojaVirtual.Infraestrutura.Data;
using Microsoft.EntityFrameworkCore;

namespace LojaVirtual.Infraestrutura.Repositories
{
	public class ParametroSistemaRepository : BaseRepository<ParametroSistema>, IParametroSistemaRepository
	{
		public ParametroSistemaRepository(LojaDbContext contexto) : base(contexto)
		{
		}

		public async Task<ParametroSistema?> ObterPorChaveAsync(string chave)
		{
			return await _contexto.ParametrosSistema
				.FirstOrDefaultAsync(p => p.Chave == chave);
		}

		public new async Task<IEnumerable<ParametroSistema>> ObterTodosAsync()
		{
			return await _contexto.ParametrosSistema
				.OrderBy(p => p.Chave)
				.ToListAsync();
		}
	}
}
