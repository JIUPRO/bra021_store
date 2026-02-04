using Microsoft.AspNetCore.Mvc;
using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Infraestrutura.Data;
using LojaVirtual.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace LojaVirtual.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class TamanhosController : ControllerBase
	{
		private readonly LojaDbContext _contexto;

		public TamanhosController(LojaDbContext contexto)
		{
			_contexto = contexto;
		}

		[HttpGet("produto/{produtoId}")]
		public async Task<ActionResult<IEnumerable<ProdutoTamanhoDTOs.GetAll>>> GetByProdutoId(Guid produtoId)
		{
			var tamanhos = await _contexto.ProdutoTamanhos
				 .Where(v => v.ProdutoId == produtoId)
				 .OrderBy(v => v.Tamanho)
				 .ToListAsync();

			var result = tamanhos.Select(v => new ProdutoTamanhoDTOs.GetAll
			{
				Id = v.Id,
				ProdutoId = v.ProdutoId,
				Tamanho = v.Tamanho,
				QuantidadeEstoque = v.QuantidadeEstoque,
				Ativo = v.Ativo
			});

			return Ok(result);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<ProdutoTamanhoDTOs.GetById>> GetById(Guid id)
		{
			var tamanho = await _contexto.ProdutoTamanhos.FindAsync(id);
			if (tamanho == null)
				return NotFound();

			var result = new ProdutoTamanhoDTOs.GetById
			{
				Id = tamanho.Id,
				ProdutoId = tamanho.ProdutoId,
				Tamanho = tamanho.Tamanho,
				QuantidadeEstoque = tamanho.QuantidadeEstoque,
				Ativo = tamanho.Ativo
			};

			return Ok(result);
		}

		[HttpPost]
		public async Task<ActionResult<ProdutoTamanhoDTOs.GetAll>> Create(ProdutoTamanhoDTOs.Create request)
		{
			try
			{
				var tamanho = new ProdutoTamanho
				{
					ProdutoId = request.ProdutoId,
					Tamanho = request.Tamanho,
					QuantidadeEstoque = 0,
					Ativo = true
				};

				_contexto.ProdutoTamanhos.Add(tamanho);
				await _contexto.SaveChangesAsync();

				var result = new ProdutoTamanhoDTOs.GetAll
				{
					Id = tamanho.Id,
					ProdutoId = tamanho.ProdutoId,
					Tamanho = tamanho.Tamanho,
					QuantidadeEstoque = tamanho.QuantidadeEstoque,
					Ativo = tamanho.Ativo
				};

				return CreatedAtAction(nameof(GetById), new { id = tamanho.Id }, result);
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(Guid id, ProdutoTamanhoDTOs.Update request)
		{
			try
			{
				var tamanho = await _contexto.ProdutoTamanhos.FindAsync(id);
				if (tamanho == null)
					return NotFound();

				tamanho.Tamanho = request.Tamanho;
				tamanho.Ativo = request.Ativo;

				_contexto.ProdutoTamanhos.Update(tamanho);
				await _contexto.SaveChangesAsync();

				return NoContent();
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			try
			{
				var tamanho = await _contexto.ProdutoTamanhos.FindAsync(id);
				if (tamanho == null)
					return NotFound();

				_contexto.ProdutoTamanhos.Remove(tamanho);
				await _contexto.SaveChangesAsync();

				return NoContent();
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}
	}
}
