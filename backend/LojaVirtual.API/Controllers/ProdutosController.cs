using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Aplicacao.Services;
using LojaVirtual.Infraestrutura.Services;

namespace LojaVirtual.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ProdutosController : ControllerBase
	{
		private readonly IProdutoService _produtoService;
		private readonly IStorageService _storageService;

		public ProdutosController(IProdutoService servicoProduto, IStorageService storageService)
		{
			_produtoService = servicoProduto;
			_storageService = storageService;
		}

		[AllowAnonymous]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<ProdutoDTO>>> ObterTodos()
		{
			var produtos = await _produtoService.ObterTodosAsync();
			return Ok(produtos);
		}

		[AllowAnonymous]
		[HttpGet("destaques")]
		public async Task<ActionResult<IEnumerable<ProdutoDTO>>> ObterDestaques()
		{
			var produtos = await _produtoService.ObterProdutosEmDestaqueAsync();
			return Ok(produtos);
		}

		[AllowAnonymous]
		[HttpGet("categoria/{categoriaId}")]
		public async Task<ActionResult<IEnumerable<ProdutoDTO>>> ObterPorCategoria(Guid categoriaId)
		{
			var produtos = await _produtoService.ObterPorCategoriaAsync(categoriaId);
			return Ok(produtos);
		}

		[AllowAnonymous]
		[HttpGet("pesquisar")]
		public async Task<ActionResult<IEnumerable<ProdutoDTO>>> Pesquisar([FromQuery] string termo)
		{
			var produtos = await _produtoService.PesquisarAsync(termo);
			return Ok(produtos);
		}

		[Authorize]
		[HttpGet("estoque-baixo")]
		public async Task<ActionResult<IEnumerable<ProdutoDTO>>> ObterEstoqueBaixo()
		{
			var produtos = await _produtoService.ObterComEstoqueBaixoAsync();
			return Ok(produtos);
		}

		[AllowAnonymous]
		[HttpGet("{id}")]
		public async Task<ActionResult<ProdutoDTO>> ObterPorId(Guid id)
		{
			var produto = await _produtoService.ObterPorIdAsync(id);
			if (produto == null)
				return NotFound(new { mensagem = "Produto não encontrado" });

			return Ok(produto);
		}

		[Authorize]
		[HttpPost]
		public async Task<ActionResult<ProdutoDTO>> Criar([FromBody] CriarProdutoDTO dto)
		{
			try
			{
				var produto = await _produtoService.CriarAsync(dto);
				return CreatedAtAction(nameof(ObterPorId), new { id = produto.Id }, produto);
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}

		[Authorize]
		[HttpPut("{id}")]
		public async Task<ActionResult<ProdutoDTO>> Atualizar(Guid id, [FromBody] AtualizarProdutoDTO dto)
		{
			if (id != dto.Id)
				return BadRequest(new { mensagem = "ID da URL não corresponde ao ID do corpo" });

			try
			{
				var produto = await _produtoService.AtualizarAsync(dto);
				if (produto == null)
					return NotFound(new { mensagem = "Produto não encontrado" });

				return Ok(produto);
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}

		[Authorize]
		[HttpPost("{id}/imagem")]
		public async Task<ActionResult<ProdutoDTO>> UploadImagem(Guid id, [FromForm] IFormFile file, CancellationToken cancellationToken)
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

			var produtoAtual = await _produtoService.ObterPorIdAsync(id);
			if (produtoAtual == null)
			{
				return NotFound(new { mensagem = "Produto não encontrado" });
			}

			try
			{
				await using var stream = file.OpenReadStream();
				var upload = await _storageService.UploadFileAsync(stream, file.FileName, file.ContentType, cancellationToken);
				var produto = await _produtoService.AtualizarImagemAsync(id, upload.Url, upload.Key);
				if (produto == null)
				{
					await _storageService.DeleteFileAsync(upload.Key, cancellationToken);
					return NotFound(new { mensagem = "Produto não encontrado" });
				}

				if (!string.IsNullOrWhiteSpace(produtoAtual.ImagemKey) && produtoAtual.ImagemKey != upload.Key)
				{
					await _storageService.DeleteFileAsync(produtoAtual.ImagemKey, cancellationToken);
				}

				return Ok(produto);
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}

		[Authorize]
		[HttpDelete("{id}/imagem")]
		public async Task<ActionResult<ProdutoDTO>> RemoverImagem(Guid id, CancellationToken cancellationToken)
		{
			var produtoAtual = await _produtoService.ObterPorIdAsync(id);
			if (produtoAtual == null)
			{
				return NotFound(new { mensagem = "Produto não encontrado" });
			}

			try
			{
				if (!string.IsNullOrWhiteSpace(produtoAtual.ImagemKey))
				{
					await _storageService.DeleteFileAsync(produtoAtual.ImagemKey, cancellationToken);
				}

				var produto = await _produtoService.AtualizarImagemAsync(id, null, null);
				return Ok(produto);
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
			var produtoAtual = await _produtoService.ObterPorIdAsync(id);
			if (produtoAtual == null)
			{
				return NotFound(new { mensagem = "Produto não encontrado" });
			}

			if (!string.IsNullOrWhiteSpace(produtoAtual.ImagemKey))
			{
				await _storageService.DeleteFileAsync(produtoAtual.ImagemKey, cancellationToken);
			}

			var resultado = await _produtoService.RemoverAsync(id);
			if (!resultado)
				return NotFound(new { mensagem = "Produto não encontrado" });

			return NoContent();
		}
	}
}
