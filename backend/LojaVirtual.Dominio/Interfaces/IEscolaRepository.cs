using LojaVirtual.Dominio.Entidades;

namespace LojaVirtual.Dominio.Interfaces
{
	public interface IEscolaRepository : IBaseRepository<Escola>
	{
		Task<IEnumerable<Escola>> ObterAtivasAsync();
	}
}
