using AutoMapper;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Enums;
using LojaVirtual.Dominio.Interfaces;
using LojaVirtual.Aplicacao.DTOs;

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
		Task<IEnumerable<ProdutoDTO>> ObterComEstoqueBaixoAsync();
	}

	public class ProdutoService : IProdutoService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapeador;
		private readonly INotificacaoService _NotificacaoService;

		public ProdutoService(IUnitOfWork unitOfWork, IMapper mapeador, INotificacaoService NotificacaoService)
		{
			_unitOfWork = unitOfWork;
			_mapeador = mapeador;
			_NotificacaoService = NotificacaoService;
		}

		public async Task<IEnumerable<ProdutoDTO>> ObterTodosAsync()
		{
			var produtos = await _unitOfWork.Produtos.ObterTodosAsync();
			return _mapeador.Map<IEnumerable<ProdutoDTO>>(produtos);
		}

		public async Task<IEnumerable<ProdutoDTO>> ObterProdutosEmDestaqueAsync()
		{
			var produtos = await _unitOfWork.Produtos.ObterProdutosEmDestaqueAsync();
			return _mapeador.Map<IEnumerable<ProdutoDTO>>(produtos);
		}

		public async Task<IEnumerable<ProdutoDTO>> ObterPorCategoriaAsync(Guid categoriaId)
		{
			var produtos = await _unitOfWork.Produtos.ObterPorCategoriaAsync(categoriaId);
			return _mapeador.Map<IEnumerable<ProdutoDTO>>(produtos);
		}

		public async Task<ProdutoDTO?> ObterPorIdAsync(Guid id)
		{
			var produto = await _unitOfWork.Produtos.ObterPorIdAsync(id);
			return produto == null ? null : _mapeador.Map<ProdutoDTO>(produto);
		}

		public async Task<IEnumerable<ProdutoDTO>> PesquisarAsync(string termo)
		{
			var produtos = await _unitOfWork.Produtos.PesquisarAsync(termo);
			return _mapeador.Map<IEnumerable<ProdutoDTO>>(produtos);
		}

		public async Task<ProdutoDTO> CriarAsync(CriarProdutoDTO dto)
		{
			var produto = _mapeador.Map<Produto>(dto);
			produto.DataCriacao = DateTime.UtcNow;
			produto.Ativo = true;

			await _unitOfWork.Produtos.AdicionarAsync(produto);
			await _unitOfWork.SalvarMudancasAsync();

			// Nota: Movimentações de estoque são registradas por ProdutoTamanho, não por Produto

			return _mapeador.Map<ProdutoDTO>(produto);
		}

		public async Task<ProdutoDTO?> AtualizarAsync(AtualizarProdutoDTO dto)
		{
			var produtoExistente = await _unitOfWork.Produtos.ObterPorIdAsync(dto.Id);
			if (produtoExistente == null)
				return null;

			_mapeador.Map(dto, produtoExistente);
			produtoExistente.DataAtualizacao = DateTime.UtcNow;

			await _unitOfWork.Produtos.AtualizarAsync(produtoExistente);
			await _unitOfWork.SalvarMudancasAsync();

			// Nota: Movimentações de estoque são registradas por ProdutoTamanho, não por Produto

			return _mapeador.Map<ProdutoDTO>(produtoExistente);
		}

		public async Task<bool> RemoverAsync(Guid id)
		{
			var produto = await _unitOfWork.Produtos.ObterPorIdAsync(id);
			if (produto == null)
				return false;

			produto.Ativo = false;
			produto.DataAtualizacao = DateTime.UtcNow;
			await _unitOfWork.Produtos.AtualizarAsync(produto);
			await _unitOfWork.SalvarMudancasAsync();

			return true;
		}

		public async Task<IEnumerable<ProdutoDTO>> ObterComEstoqueBaixoAsync()
		{
			var produtos = await _unitOfWork.Produtos.ObterComEstoqueBaixoAsync();
			return _mapeador.Map<IEnumerable<ProdutoDTO>>(produtos);
		}
	}
}
