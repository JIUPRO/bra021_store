using Microsoft.AspNetCore.Mvc;
using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Aplicacao.Services;

namespace LojaVirtual.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class CategoriasController : ControllerBase
	{
		private readonly ICategoriaService _categoriaService;

		public CategoriasController(ICategoriaService servicoCategoria)
		{
			_categoriaService = servicoCategoria;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<CategoriaDTO>>> ObterTodas()
		{
			var categorias = await _categoriaService.ObterTodosComProdutosAsync();
			return Ok(categorias);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<CategoriaDTO>> ObterPorId(Guid id)
		{
			var categoria = await _categoriaService.ObterPorIdAsync(id);
			if (categoria == null)
				return NotFound(new { mensagem = "Categoria não encontrada" });

			return Ok(categoria);
		}

		[HttpGet("{id}/produtos")]
		public async Task<ActionResult<CategoriaDTO>> ObterComProdutos(Guid id)
		{
			var categoria = await _categoriaService.ObterComProdutosAsync(id);
			if (categoria == null)
				return NotFound(new { mensagem = "Categoria não encontrada" });

			return Ok(categoria);
		}

		[HttpPost]
		public async Task<ActionResult<CategoriaDTO>> Criar([FromBody] CriarCategoriaDTO dto)
		{
			try
			{
				var categoria = await _categoriaService.CriarAsync(dto);
				return CreatedAtAction(nameof(ObterPorId), new { id = categoria.Id }, categoria);
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}

		[HttpPut("{id}")]
		public async Task<ActionResult<CategoriaDTO>> Atualizar(Guid id, [FromBody] AtualizarCategoriaDTO dto)
		{
			if (id != dto.Id)
				return BadRequest(new { mensagem = "ID da URL não corresponde ao ID do corpo" });

			try
			{
				var categoria = await _categoriaService.AtualizarAsync(dto);
				if (categoria == null)
					return NotFound(new { mensagem = "Categoria não encontrada" });

				return Ok(categoria);
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Remover(Guid id)
		{
			var resultado = await _categoriaService.RemoverAsync(id);
			if (!resultado)
				return BadRequest(new { mensagem = "Não foi possível remover a categoria. Verifique se há produtos associados." });

			return NoContent();
		}
	}
}
