using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Interfaces;
using Mapster;

namespace LojaVirtual.Aplicacao.Services
{
	public interface IProdutoService
	{
		Task<IEnumerable<ProdutoDTO>> ObterTodosAsync();
		Task<IEnumerable<ProdutoDTO>> ObterProdutosEmDestaqueAsync();
		Task<IEnumerable<ProdutoDTO>> ObterPorCategoriaAsync(Guid categoriaId);
		Task<ProdutoDTO?> ObterPorIdAsync(Guid id);
		Task<IEnumerable<ProdutoDTO>> PesquisarAsync(string termo);
		Task<ProdutoDTO> CriarAsync(CriarProdutoDTO dto);
		Task<ProdutoDTO?> AtualizarAsync(AtualizarProdutoDTO dto);
		Task<bool> RemoverAsync(Guid id);
		Task<ProdutoDTO?> AtualizarImagemAsync(Guid id, string? imagemUrl, string? imagemKey);
		Task<IEnumerable<ProdutoDTO>> ObterComEstoqueBaixoAsync();
	}

	public class ProdutoService : IProdutoService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly INotificacaoService _NotificacaoService;

		public ProdutoService(IUnitOfWork unitOfWork, INotificacaoService notificacaoService)
		{
			_unitOfWork = unitOfWork;
			_NotificacaoService = notificacaoService;
		}

		public async Task<IEnumerable<ProdutoDTO>> ObterTodosAsync()
		{
			var produtos = await _unitOfWork.Produtos.ObterTodosAsync();
			return produtos.Adapt<IEnumerable<ProdutoDTO>>();
		}

		public async Task<IEnumerable<ProdutoDTO>> ObterProdutosEmDestaqueAsync()
		{
			var produtos = await _unitOfWork.Produtos.ObterProdutosEmDestaqueAsync();
			return produtos.Adapt<IEnumerable<ProdutoDTO>>();
		}

		public async Task<IEnumerable<ProdutoDTO>> ObterPorCategoriaAsync(Guid categoriaId)
		{
			var produtos = await _unitOfWork.Produtos.ObterPorCategoriaAsync(categoriaId);
			return produtos.Adapt<IEnumerable<ProdutoDTO>>();
		}

		public async Task<ProdutoDTO?> ObterPorIdAsync(Guid id)
		{
			var produto = await _unitOfWork.Produtos.ObterPorIdAsync(id);
			return produto?.Adapt<ProdutoDTO>();
		}

		public async Task<IEnumerable<ProdutoDTO>> PesquisarAsync(string termo)
		{
			var produtos = await _unitOfWork.Produtos.PesquisarAsync(termo);
			return produtos.Adapt<IEnumerable<ProdutoDTO>>();
		}

		public async Task<ProdutoDTO> CriarAsync(CriarProdutoDTO dto)
		{
			var produto = dto.Adapt<Produto>();
			produto.DataCriacao = DateTime.UtcNow;
			produto.Ativo = true;

			await _unitOfWork.Produtos.AdicionarAsync(produto);
			await _unitOfWork.SalvarMudancasAsync();

			return produto.Adapt<ProdutoDTO>();
		}

		public async Task<ProdutoDTO?> AtualizarAsync(AtualizarProdutoDTO dto)
		{
			var produtoExistente = await _unitOfWork.Produtos.ObterPorIdAsync(dto.Id);
			if (produtoExistente == null)
			{
				return null;
			}

			dto.Adapt(produtoExistente);
			produtoExistente.DataAtualizacao = DateTime.UtcNow;

			await _unitOfWork.Produtos.AtualizarAsync(produtoExistente);
			await _unitOfWork.SalvarMudancasAsync();

			return produtoExistente.Adapt<ProdutoDTO>();
		}

		public async Task<bool> RemoverAsync(Guid id)
		{
			var produto = await _unitOfWork.Produtos.ObterPorIdAsync(id);
			if (produto == null)
			{
				return false;
			}

			produto.Ativo = false;
			produto.DataAtualizacao = DateTime.UtcNow;
			await _unitOfWork.Produtos.AtualizarAsync(produto);
			await _unitOfWork.SalvarMudancasAsync();

			return true;
		}

		public async Task<ProdutoDTO?> AtualizarImagemAsync(Guid id, string? imagemUrl, string? imagemKey)
		{
			var produto = await _unitOfWork.Produtos.ObterPorIdAsync(id);
			if (produto == null)
			{
				return null;
			}

			produto.ImagemUrl = imagemUrl;
			produto.ImagemKey = imagemKey;
			produto.DataAtualizacao = DateTime.UtcNow;

			await _unitOfWork.Produtos.AtualizarAsync(produto);
			await _unitOfWork.SalvarMudancasAsync();

			return produto.Adapt<ProdutoDTO>();
		}

		public async Task<IEnumerable<ProdutoDTO>> ObterComEstoqueBaixoAsync()
		{
			var produtos = await _unitOfWork.Produtos.ObterComEstoqueBaixoAsync();
			return produtos.Adapt<IEnumerable<ProdutoDTO>>();
		}
	}
}
