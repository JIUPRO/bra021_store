using LojaVirtual.Dominio.Entidades;

namespace LojaVirtual.Dominio.Interfaces
{
	public interface IUsuarioRepository : IBaseRepository<Usuario>
	{
		Task<Usuario?> ObterPorEmailAsync(string email);
		Task<bool> VerificarEmailExisteAsync(string email);
	}
}
