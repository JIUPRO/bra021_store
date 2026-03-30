using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Interfaces;
using Mapster;

namespace LojaVirtual.Aplicacao.Services
{
	public interface ICategoriaService
	{
		Task<IEnumerable<CategoriaDTO>> ObterTodosAsync();
		Task<IEnumerable<CategoriaDTO>> ObterTodosComProdutosAsync();
		Task<CategoriaDTO?> ObterPorIdAsync(Guid id);
		Task<CategoriaDTO?> ObterComProdutosAsync(Guid id);
		Task<CategoriaDTO> CriarAsync(CriarCategoriaDTO dto);
		Task<CategoriaDTO?> AtualizarAsync(AtualizarCategoriaDTO dto);
		Task<bool> RemoverAsync(Guid id);
	}

	public class CategoriaService : ICategoriaService
	{
		private readonly IUnitOfWork _unitOfWork;

		public CategoriaService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<IEnumerable<CategoriaDTO>> ObterTodosAsync()
		{
			var categorias = await _unitOfWork.Categorias.ObterTodosAsync();
			return categorias.Adapt<IEnumerable<CategoriaDTO>>();
		}

		public async Task<IEnumerable<CategoriaDTO>> ObterTodosComProdutosAsync()
		{
			var categorias = await _unitOfWork.Categorias.ObterTodosAsync();
			var resultado = new List<CategoriaDTO>();

			foreach (var categoria in categorias)
			{
				var categoriaComProdutos = await _unitOfWork.Categorias.ObterComProdutosAsync(categoria.Id);
				var dto = categoria.Adapt<CategoriaDTO>();
				if (categoriaComProdutos != null)
				{
					dto.QuantidadeProdutos = categoriaComProdutos.Produtos.Count(p => p.Ativo);
				}
				resultado.Add(dto);
			}

			return resultado;
		}

		public async Task<CategoriaDTO?> ObterComProdutosAsync(Guid id)
		{
			var categoria = await _unitOfWork.Categorias.ObterComProdutosAsync(id);
			return categoria?.Adapt<CategoriaDTO>();
		}

		public async Task<CategoriaDTO?> ObterPorIdAsync(Guid id)
		{
			var categoria = await _unitOfWork.Categorias.ObterPorIdAsync(id);
			return categoria?.Adapt<CategoriaDTO>();
		}

		public async Task<CategoriaDTO> CriarAsync(CriarCategoriaDTO dto)
		{
			var categoria = dto.Adapt<Categoria>();
			categoria.DataCriacao = DateTime.UtcNow;
			categoria.Ativo = true;

			await _unitOfWork.Categorias.AdicionarAsync(categoria);
			await _unitOfWork.SalvarMudancasAsync();

			return categoria.Adapt<CategoriaDTO>();
		}

		public async Task<CategoriaDTO?> AtualizarAsync(AtualizarCategoriaDTO dto)
		{
			var categoriaExistente = await _unitOfWork.Categorias.ObterPorIdAsync(dto.Id);
			if (categoriaExistente == null)
			{
				return null;
			}

			dto.Adapt(categoriaExistente);
			categoriaExistente.DataAtualizacao = DateTime.UtcNow;

			await _unitOfWork.Categorias.AtualizarAsync(categoriaExistente);
			await _unitOfWork.SalvarMudancasAsync();

			return categoriaExistente.Adapt<CategoriaDTO>();
		}

		public async Task<bool> RemoverAsync(Guid id)
		{
			var categoria = await _unitOfWork.Categorias.ObterPorIdAsync(id);
			if (categoria == null)
			{
				return false;
			}

			var categoriaComProdutos = await _unitOfWork.Categorias.ObterComProdutosAsync(id);
			if (categoriaComProdutos?.Produtos.Any(p => p.Ativo) == true)
			{
				return false;
			}

			categoria.Ativo = false;
			categoria.DataAtualizacao = DateTime.UtcNow;
			await _unitOfWork.Categorias.AtualizarAsync(categoria);
			await _unitOfWork.SalvarMudancasAsync();

			return true;
		}
	}
}
