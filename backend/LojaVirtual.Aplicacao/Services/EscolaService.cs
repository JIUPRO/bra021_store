using AutoMapper;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Interfaces;
using LojaVirtual.Aplicacao.DTOs;

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
		private readonly IMapper _mapeador;

		public EscolaService(IUnitOfWork unitOfWork, IMapper mapeador)
		{
			_unitOfWork = unitOfWork;
			_mapeador = mapeador;
		}

		public async Task<IEnumerable<EscolaDTO>> ObterTodasAsync()
		{
			var escolas = await _unitOfWork.Escolas.ObterTodosAsync();
			return _mapeador.Map<IEnumerable<EscolaDTO>>(escolas);
		}

		public async Task<IEnumerable<EscolaDTO>> ObterAtivasAsync()
		{
			var escolas = await _unitOfWork.Escolas.ObterAtivasAsync();
			return _mapeador.Map<IEnumerable<EscolaDTO>>(escolas);
		}

		public async Task<EscolaDTO?> ObterPorIdAsync(Guid id)
		{
			var escola = await _unitOfWork.Escolas.ObterPorIdAsync(id);
			return escola == null ? null : _mapeador.Map<EscolaDTO>(escola);
		}

		public async Task<EscolaDTO> CriarAsync(CriarEscolaDTO dto)
		{
			var escola = _mapeador.Map<Escola>(dto);
			escola.DataCriacao = DateTime.UtcNow;
			escola.Ativo = true;

			await _unitOfWork.Escolas.AdicionarAsync(escola);
			await _unitOfWork.SalvarMudancasAsync();

			return _mapeador.Map<EscolaDTO>(escola);
		}

		public async Task<EscolaDTO?> AtualizarAsync(AtualizarEscolaDTO dto)
		{
			var escolaExistente = await _unitOfWork.Escolas.ObterPorIdAsync(dto.Id);
			if (escolaExistente == null)
				return null;

			_mapeador.Map(dto, escolaExistente);
			escolaExistente.DataAtualizacao = DateTime.UtcNow;

			await _unitOfWork.Escolas.AtualizarAsync(escolaExistente);
			await _unitOfWork.SalvarMudancasAsync();

			return _mapeador.Map<EscolaDTO>(escolaExistente);
		}

		public async Task<bool> RemoverAsync(Guid id)
		{
			var escola = await _unitOfWork.Escolas.ObterPorIdAsync(id);
			if (escola == null)
				return false;

			escola.Ativo = false;
			escola.DataAtualizacao = DateTime.UtcNow;
			await _unitOfWork.Escolas.AtualizarAsync(escola);
			await _unitOfWork.SalvarMudancasAsync();

			return true;
		}
	}
}
