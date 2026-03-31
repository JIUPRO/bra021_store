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
			var alterou = false;

			alterou |= await GarantirParametroAsync(parametros, "EmailAdministrador", string.Empty, "Email do administrador para receber resumo de pedidos", "Email", "Email");
			alterou |= await GarantirParametroAsync(parametros, "FreteHabilitado", "false", "Liga ou desliga a rotina de frete configurável no checkout", "Boolean", "Frete");
			alterou |= await GarantirParametroAsync(parametros, "FreteProvider", "Fixo", "Define se o checkout usará frete fixo ou integração com Melhor Envio", "Lista", "Frete");
			alterou |= await GarantirParametroAsync(parametros, "FreteCepOrigem", string.Empty, "CEP de origem usado para cotação dinâmica de frete", "Cep", "Frete");
			alterou |= await GarantirParametroAsync(parametros, "FretePrazoPreparacaoDias", "0", "Prazo interno em dias para separação, emissão e postagem antes do prazo da transportadora", "Numero", "Frete");
			alterou |= await GarantirParametroAsync(parametros, "MelhorEnvioNaoComercial", "true", "Define se a etiqueta será gerada como envio não comercial para testes/sandbox", "Boolean", "Melhor Envio");
			alterou |= await GarantirParametroAsync(parametros, "MelhorEnvioRemetenteNome", string.Empty, "Nome do remetente usado na geração da etiqueta do Melhor Envio", "String", "Melhor Envio");
			alterou |= await GarantirParametroAsync(parametros, "MelhorEnvioRemetenteTelefone", string.Empty, "Telefone do remetente usado na geração da etiqueta do Melhor Envio", "String", "Melhor Envio");
			alterou |= await GarantirParametroAsync(parametros, "MelhorEnvioRemetenteEmail", string.Empty, "Email do remetente usado na geração da etiqueta do Melhor Envio", "String", "Melhor Envio");
			alterou |= await GarantirParametroAsync(parametros, "MelhorEnvioRemetenteDocumento", string.Empty, "CPF ou CNPJ do remetente usado na geração da etiqueta do Melhor Envio", "String", "Melhor Envio");
			alterou |= await GarantirParametroAsync(parametros, "MelhorEnvioRemetenteInscricaoEstadual", "ISENTO", "Inscrição estadual do remetente usada na geração da etiqueta do Melhor Envio", "String", "Melhor Envio");
			alterou |= await GarantirParametroAsync(parametros, "MelhorEnvioRemetenteLogradouro", string.Empty, "Logradouro do remetente usado na geração da etiqueta do Melhor Envio", "String", "Melhor Envio");
			alterou |= await GarantirParametroAsync(parametros, "MelhorEnvioRemetenteNumero", string.Empty, "Número do endereço do remetente usado na geração da etiqueta do Melhor Envio", "String", "Melhor Envio");
			alterou |= await GarantirParametroAsync(parametros, "MelhorEnvioRemetenteComplemento", string.Empty, "Complemento do endereço do remetente usado na geração da etiqueta do Melhor Envio", "String", "Melhor Envio");
			alterou |= await GarantirParametroAsync(parametros, "MelhorEnvioRemetenteBairro", string.Empty, "Bairro do remetente usado na geração da etiqueta do Melhor Envio", "String", "Melhor Envio");
			alterou |= await GarantirParametroAsync(parametros, "MelhorEnvioRemetenteCidade", string.Empty, "Cidade do remetente usada na geração da etiqueta do Melhor Envio", "String", "Melhor Envio");
			alterou |= await GarantirParametroAsync(parametros, "MelhorEnvioRemetenteEstado", string.Empty, "UF do remetente usada na geração da etiqueta do Melhor Envio", "String", "Melhor Envio");

			if (alterou)
			{
				await _unitOfWork.SalvarMudancasAsync();
				parametros = (await _unitOfWork.ParametrosSistema.ObterTodosAsync()).ToList();
			}

			return parametros.Adapt<IEnumerable<ParametroSistemaDTO>>();
		}

		private async Task<bool> GarantirParametroAsync(
			List<ParametroSistema> parametros,
			string chave,
			string valor,
			string descricao,
			string tipo,
			string? secao)
		{
			if (parametros.Any(p => p.Chave == chave))
			{
				return false;
			}

			var parametro = new ParametroSistema
			{
				Chave = chave,
				Valor = valor,
				Descricao = descricao,
				Secao = secao,
				Tipo = tipo
			};

			await _unitOfWork.ParametrosSistema.AdicionarAsync(parametro);
			parametros.Add(parametro);
			return true;
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
				Secao = dto.Secao,
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
			parametro.Secao = dto.Secao;
			parametro.DataAtualizacao = DateTime.UtcNow;

			await _unitOfWork.SalvarMudancasAsync();

			return parametro.Adapt<ParametroSistemaDTO>();
		}
	}
}
