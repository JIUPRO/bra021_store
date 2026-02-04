using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Aplicacao.Services;
using Microsoft.AspNetCore.Mvc;

namespace LojaVirtual.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ParametrosSistemaController : ControllerBase
	{
		private readonly IParametroSistemaService _parametroSistemaService;

		public ParametrosSistemaController(IParametroSistemaService servicoParametroSistema)
		{
			_parametroSistemaService = servicoParametroSistema;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<ParametroSistemaDTO>>> ObterTodos()
		{
			var parametros = await _parametroSistemaService.ObterTodosAsync();
			return Ok(parametros);
		}

		[HttpGet("chave/{chave}")]
		public async Task<ActionResult<ParametroSistemaDTO>> ObterPorChave(string chave)
		{
			var parametro = await _parametroSistemaService.ObterPorChaveAsync(chave);
			if (parametro == null)
			{
				// Se for MaximoParcelas e não existir, retornar valor padrão
				if (chave == "MaximoParcelas")
				{
					return Ok(new ParametroSistemaDTO
					{
						Id = Guid.Empty,
						Chave = "MaximoParcelas",
						Valor = "3",
						Descricao = "Número máximo de parcelas sem juros no cartão de crédito"
					});
				}
				return NotFound(new { mensagem = "Parâmetro não encontrado" });
			}

			return Ok(parametro);
		}

		[HttpPost]
		public async Task<ActionResult<ParametroSistemaDTO>> Criar([FromBody] CriarParametroSistemaDTO dto)
		{
			try
			{
				var parametro = await _parametroSistemaService.CriarAsync(dto);
				return CreatedAtAction(nameof(ObterPorChave), new { chave = parametro.Chave }, parametro);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}

		[HttpPut("{id}")]
		public async Task<ActionResult<ParametroSistemaDTO>> Atualizar(Guid id, [FromBody] AtualizarParametroSistemaDTO dto)
		{
			if (dto == null || string.IsNullOrEmpty(dto.Valor))
				return BadRequest(new { mensagem = "Valor é obrigatório" });

			try
			{
				var parametro = await _parametroSistemaService.AtualizarAsync(id, dto);
				return Ok(parametro);
			}
			catch (KeyNotFoundException)
			{
				return NotFound(new { mensagem = "Parâmetro não encontrado" });
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = $"Erro: {ex.Message}" });
			}
		}
	}
}
