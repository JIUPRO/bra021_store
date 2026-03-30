using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Interfaces;
using Mapster;

namespace LojaVirtual.Aplicacao.Services
{
	public interface IEscolaService
	{
		Task<IEnumerable<EscolaDTO>> ObterTodasAsync();
		Task<IEnumerable<EscolaDTO>> ObterAtivasAsync();
		Task<EscolaDTO?> ObterPorIdAsync(Guid id);
		Task<EscolaDTO> CriarAsync(CriarEscolaDTO dto);
		Task<EscolaDTO?> AtualizarAsync(AtualizarEscolaDTO dto);
		Task<bool> RemoverAsync(Guid id);
	}

	public class EscolaService : IEscolaService
	{
		private readonly IUnitOfWork _unitOfWork;

		public EscolaService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<IEnumerable<EscolaDTO>> ObterTodasAsync()
		{
			var escolas = await _unitOfWork.Escolas.ObterTodosAsync();
			return escolas.Adapt<IEnumerable<EscolaDTO>>();
		}

		public async Task<IEnumerable<EscolaDTO>> ObterAtivasAsync()
		{
			var escolas = await _unitOfWork.Escolas.ObterAtivasAsync();
			return escolas.Adapt<IEnumerable<EscolaDTO>>();
		}

		public async Task<EscolaDTO?> ObterPorIdAsync(Guid id)
		{
			var escola = await _unitOfWork.Escolas.ObterPorIdAsync(id);
			return escola?.Adapt<EscolaDTO>();
		}

		public async Task<EscolaDTO> CriarAsync(CriarEscolaDTO dto)
		{
			var escola = dto.Adapt<Escola>();
			escola.DataCriacao = DateTime.UtcNow;
			escola.Ativo = true;

			await _unitOfWork.Escolas.AdicionarAsync(escola);
			await _unitOfWork.SalvarMudancasAsync();

			return escola.Adapt<EscolaDTO>();
		}

		public async Task<EscolaDTO?> AtualizarAsync(AtualizarEscolaDTO dto)
		{
			var escolaExistente = await _unitOfWork.Escolas.ObterPorIdAsync(dto.Id);
			if (escolaExistente == null)
			{
				return null;
			}

			dto.Adapt(escolaExistente);
			escolaExistente.DataAtualizacao = DateTime.UtcNow;

			await _unitOfWork.Escolas.AtualizarAsync(escolaExistente);
			await _unitOfWork.SalvarMudancasAsync();

			return escolaExistente.Adapt<EscolaDTO>();
		}

		public async Task<bool> RemoverAsync(Guid id)
		{
			var escola = await _unitOfWork.Escolas.ObterPorIdAsync(id);
			if (escola == null)
			{
				return false;
			}

			escola.Ativo = false;
			escola.DataAtualizacao = DateTime.UtcNow;
			await _unitOfWork.Escolas.AtualizarAsync(escola);
			await _unitOfWork.SalvarMudancasAsync();

			return true;
		}
	}
}
