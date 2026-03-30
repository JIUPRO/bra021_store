using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Interfaces;
using Mapster;

namespace LojaVirtual.Aplicacao.Services
{
	public class ParametroSistemaService : IParametroSistemaService
	{
		private readonly IUnitOfWork _unitOfWork;

		public ParametroSistemaService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<IEnumerable<ParametroSistemaDTO>> ObterTodosAsync()
		{
			var parametros = (await _unitOfWork.ParametrosSistema.ObterTodosAsync()).ToList();

			if (!parametros.Any(p => p.Chave == "EmailAdministrador"))
			{
				await _unitOfWork.ParametrosSistema.AdicionarAsync(new ParametroSistema
				{
					Chave = "EmailAdministrador",
					Valor = string.Empty,
					Descricao = "Email do administrador para receber resumo de pedidos",
					Tipo = "Email"
				});
				await _unitOfWork.SalvarMudancasAsync();
				parametros = (await _unitOfWork.ParametrosSistema.ObterTodosAsync()).ToList();
			}

			return parametros.Adapt<IEnumerable<ParametroSistemaDTO>>();
		}

		public async Task<ParametroSistemaDTO?> ObterPorChaveAsync(string chave)
		{
			var parametro = await _unitOfWork.ParametrosSistema.ObterPorChaveAsync(chave);
			return parametro?.Adapt<ParametroSistemaDTO>();
		}

		public async Task<ParametroSistemaDTO> CriarAsync(CriarParametroSistemaDTO dto)
		{
			var existente = await _unitOfWork.ParametrosSistema.ObterPorChaveAsync(dto.Chave);
			if (existente != null)
			{
				throw new InvalidOperationException($"Já existe um parâmetro com a chave '{dto.Chave}'");
			}

			var parametro = new ParametroSistema
			{
				Chave = dto.Chave,
				Valor = dto.Valor,
				Descricao = dto.Descricao ?? string.Empty,
				Tipo = "String"
			};

			await _unitOfWork.ParametrosSistema.AdicionarAsync(parametro);
			await _unitOfWork.SalvarMudancasAsync();

			return parametro.Adapt<ParametroSistemaDTO>();
		}

		public async Task<ParametroSistemaDTO> AtualizarAsync(Guid id, AtualizarParametroSistemaDTO dto)
		{
			var parametro = await _unitOfWork.ParametrosSistema.ObterPorIdAsync(id);
			if (parametro == null)
			{
				throw new KeyNotFoundException("Parâmetro não encontrado");
			}

			parametro.Valor = dto.Valor;
			parametro.DataAtualizacao = DateTime.UtcNow;

			await _unitOfWork.SalvarMudancasAsync();

			return parametro.Adapt<ParametroSistemaDTO>();
		}
	}
}
