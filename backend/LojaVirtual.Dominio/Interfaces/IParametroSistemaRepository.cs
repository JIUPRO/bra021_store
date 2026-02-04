using LojaVirtual.Dominio.Entidades;

namespace LojaVirtual.Dominio.Interfaces
{
	public interface IParametroSistemaRepository : IBaseRepository<ParametroSistema>
	{
		Task<ParametroSistema?> ObterPorChaveAsync(string chave);
		Task<IEnumerable<ParametroSistema>> ObterTodosAsync();
	}
}
