using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Aplicacao.Services;
using LojaVirtual.Infraestrutura.Services;

namespace LojaVirtual.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class CategoriasController : ControllerBase
	{
		private readonly ICategoriaService _categoriaService;
		private readonly IStorageService _storageService;

		public CategoriasController(ICategoriaService servicoCategoria, IStorageService storageService)
		{
			_categoriaService = servicoCategoria;
			_storageService = storageService;
		}

		[AllowAnonymous]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<CategoriaDTO>>> ObterTodas()
		{
			var categorias = await _categoriaService.ObterTodosComProdutosAsync();
			return Ok(categorias);
		}

		[AllowAnonymous]
		[HttpGet("{id}")]
		public async Task<ActionResult<CategoriaDTO>> ObterPorId(Guid id)
		{
			var categoria = await _categoriaService.ObterPorIdAsync(id);
			if (categoria == null)
				return NotFound(new { mensagem = "Categoria não encontrada" });

			return Ok(categoria);
		}

		[AllowAnonymous]
		[HttpGet("{id}/produtos")]
		public async Task<ActionResult<CategoriaDTO>> ObterComProdutos(Guid id)
		{
			var categoria = await _categoriaService.ObterComProdutosAsync(id);
			if (categoria == null)
				return NotFound(new { mensagem = "Categoria não encontrada" });

			return Ok(categoria);
		}

		[Authorize]
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

		[Authorize]
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

		[Authorize]
		[HttpPost("{id}/imagem")]
		public async Task<ActionResult<CategoriaDTO>> UploadImagem(Guid id, [FromForm] IFormFile file, CancellationToken cancellationToken)
		{
			if (file == null || file.Length == 0)
			{
				return BadRequest(new { mensagem = "Selecione uma imagem para enviar." });
			}

			var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
			if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
			{
				return BadRequest(new { mensagem = "Envie apenas arquivos JPG, PNG ou WEBP." });
			}

			var categoriaAtual = await _categoriaService.ObterPorIdAsync(id);
			if (categoriaAtual == null)
			{
				return NotFound(new { mensagem = "Categoria não encontrada" });
			}

			try
			{
				await using var stream = file.OpenReadStream();
				var upload = await _storageService.UploadFileAsync(stream, file.FileName, file.ContentType, cancellationToken);
				var categoria = await _categoriaService.AtualizarImagemAsync(id, upload.Url, upload.Key);
				if (categoria == null)
				{
					await _storageService.DeleteFileAsync(upload.Key, cancellationToken);
					return NotFound(new { mensagem = "Categoria não encontrada" });
				}

				if (!string.IsNullOrWhiteSpace(categoriaAtual.ImagemKey) && categoriaAtual.ImagemKey != upload.Key)
				{
					await _storageService.DeleteFileAsync(categoriaAtual.ImagemKey, cancellationToken);
				}

				return Ok(categoria);
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}

		[Authorize]
		[HttpDelete("{id}/imagem")]
		public async Task<ActionResult<CategoriaDTO>> RemoverImagem(Guid id, CancellationToken cancellationToken)
		{
			var categoriaAtual = await _categoriaService.ObterPorIdAsync(id);
			if (categoriaAtual == null)
			{
				return NotFound(new { mensagem = "Categoria não encontrada" });
			}

			try
			{
				if (!string.IsNullOrWhiteSpace(categoriaAtual.ImagemKey))
				{
					await _storageService.DeleteFileAsync(categoriaAtual.ImagemKey, cancellationToken);
				}

				var categoria = await _categoriaService.AtualizarImagemAsync(id, null, null);
				if (categoria == null)
				{
					return NotFound(new { mensagem = "Categoria não encontrada" });
				}

				return Ok(categoria);
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}

		[Authorize]
		[HttpDelete("{id}")]
		public async Task<IActionResult> Remover(Guid id, CancellationToken cancellationToken)
		{
			var categoriaAtual = await _categoriaService.ObterPorIdAsync(id);
			if (categoriaAtual == null)
			{
				return NotFound(new { mensagem = "Categoria não encontrada" });
			}

			var resultado = await _categoriaService.RemoverAsync(id);
			if (!resultado)
				return BadRequest(new { mensagem = "Não foi possível remover a categoria. Verifique se há produtos associados." });

			if (!string.IsNullOrWhiteSpace(categoriaAtual.ImagemKey))
			{
				await _storageService.DeleteFileAsync(categoriaAtual.ImagemKey, cancellationToken);
			}

			return NoContent();
		}
	}
}
