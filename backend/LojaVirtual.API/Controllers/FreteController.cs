using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Aplicacao.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LojaVirtual.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class FreteController : ControllerBase
	{
		private readonly IFreteService _freteService;

		public FreteController(IFreteService freteService)
		{
			_freteService = freteService;
		}

		[AllowAnonymous]
		[HttpPost("cotar")]
		public async Task<ActionResult<CotacaoFreteResponseDTO>> Cotar([FromBody] CotacaoFreteRequestDTO dto, CancellationToken cancellationToken)
		{
			if (dto.Itens.Count == 0)
			{
				return BadRequest(new { mensagem = "Informe ao menos um item para cotação." });
			}

			var cotacao = await _freteService.CotarAsync(dto, cancellationToken);
			return Ok(cotacao);
		}
	}
}
