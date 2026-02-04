using Microsoft.EntityFrameworkCore;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Interfaces;
using LojaVirtual.Infraestrutura.Data;

namespace LojaVirtual.Infraestrutura.Repositories
{
	public class EscolaRepository : BaseRepository<Escola>, IEscolaRepository
	{
		public EscolaRepository(LojaDbContext contexto) : base(contexto)
		{
		}

		public async Task<IEnumerable<Escola>> ObterAtivasAsync()
		{
			return await _contexto.Escolas
				 .Where(e => e.Ativo)
				 .OrderBy(e => e.Nome)
				 .ToListAsync();
		}
	}
}
