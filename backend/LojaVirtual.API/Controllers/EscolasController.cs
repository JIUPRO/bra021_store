using Microsoft.AspNetCore.Mvc;
using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Aplicacao.Services;

namespace LojaVirtual.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class EscolasController : ControllerBase
	{
		private readonly IEscolaService _escolaService;

		public EscolasController(IEscolaService servicoEscola)
		{
			_escolaService = servicoEscola;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<EscolaDTO>>> ObterTodas()
		{
			var escolas = await _escolaService.ObterTodasAsync();
			return Ok(escolas);
		}

		[HttpGet("ativas")]
		public async Task<ActionResult<IEnumerable<EscolaDTO>>> ObterAtivas()
		{
			var escolas = await _escolaService.ObterAtivasAsync();
			return Ok(escolas);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<EscolaDTO>> ObterPorId(Guid id)
		{
			var escola = await _escolaService.ObterPorIdAsync(id);
			if (escola == null)
				return NotFound(new { mensagem = "Escola não encontrada" });

			return Ok(escola);
		}

		[HttpPost]
		public async Task<ActionResult<EscolaDTO>> Criar([FromBody] CriarEscolaDTO dto)
		{
			try
			{
				var escola = await _escolaService.CriarAsync(dto);
				return CreatedAtAction(nameof(ObterPorId), new { id = escola.Id }, escola);
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}

		[HttpPut("{id}")]
		public async Task<ActionResult<EscolaDTO>> Atualizar(Guid id, [FromBody] AtualizarEscolaDTO dto)
		{
			if (id != dto.Id)
				return BadRequest(new { mensagem = "ID da URL não corresponde ao ID do corpo" });

			try
			{
				var escola = await _escolaService.AtualizarAsync(dto);
				if (escola == null)
					return NotFound(new { mensagem = "Escola não encontrada" });

				return Ok(escola);
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Remover(Guid id)
		{
			var resultado = await _escolaService.RemoverAsync(id);
			if (!resultado)
				return NotFound(new { mensagem = "Escola não encontrada" });

			return NoContent();
		}
	}
}
