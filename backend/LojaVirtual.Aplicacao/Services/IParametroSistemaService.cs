using LojaVirtual.Aplicacao.DTOs;

namespace LojaVirtual.Aplicacao.Services
{
	public interface IParametroSistemaService
	{
		Task<IEnumerable<ParametroSistemaDTO>> ObterTodosAsync();
		Task<ParametroSistemaDTO?> ObterPorChaveAsync(string chave);
		Task<ParametroSistemaDTO> CriarAsync(CriarParametroSistemaDTO dto);
		Task<ParametroSistemaDTO> AtualizarAsync(Guid id, AtualizarParametroSistemaDTO dto);
	}
}
