using AutoMapper;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Enums;
using LojaVirtual.Dominio.Interfaces;
using LojaVirtual.Aplicacao.DTOs;

namespace LojaVirtual.Aplicacao.Services
{
	public interface IPedidoService
	{
		Task<IEnumerable<ResumoPedidoDTO>> ObterTodosAsync();
		Task<PedidoDTO?> ObterPorIdAsync(Guid id);
		Task<IEnumerable<ResumoPedidoDTO>> ObterPorClienteAsync(Guid clienteId);
		Task<IEnumerable<ResumoPedidoDTO>> ObterPorStatusAsync(StatusPedido status);
		Task<IEnumerable<ResumoPedidoDTO>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
		Task<PedidoDTO> CriarAsync(CriarPedidoDTO dto);
		Task<PedidoDTO?> AtualizarStatusAsync(AtualizarStatusPedidoDTO dto);
		Task<PedidoDTO?> AtualizarNotaFiscalAsync(AtualizarNotaFiscalDTO dto);
	}

	public class PedidoService : IPedidoService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapeador;
		private readonly INotificacaoService _NotificacaoService;

		public PedidoService(IUnitOfWork unitOfWork, IMapper mapeador, INotificacaoService NotificacaoService)
		{
			_unitOfWork = unitOfWork;
			_mapeador = mapeador;
			_NotificacaoService = NotificacaoService;
		}

		public async Task<IEnumerable<ResumoPedidoDTO>> ObterTodosAsync()
		{
			var pedidos = await _unitOfWork.Pedidos.ObterTodosAsync();
			return _mapeador.Map<IEnumerable<ResumoPedidoDTO>>(pedidos);
		}

		public async Task<PedidoDTO?> ObterPorIdAsync(Guid id)
		{
			var pedido = await _unitOfWork.Pedidos.ObterComItensAsync(id);
			return pedido == null ? null : _mapeador.Map<PedidoDTO>(pedido);
		}

		public async Task<IEnumerable<ResumoPedidoDTO>> ObterPorClienteAsync(Guid clienteId)
		{
			var pedidos = await _unitOfWork.Pedidos.ObterPorClienteAsync(clienteId);
			return _mapeador.Map<IEnumerable<ResumoPedidoDTO>>(pedidos);
		}

		public async Task<IEnumerable<ResumoPedidoDTO>> ObterPorStatusAsync(StatusPedido status)
		{
			var pedidos = await _unitOfWork.Pedidos.ObterPorStatusAsync(status);
			return _mapeador.Map<IEnumerable<ResumoPedidoDTO>>(pedidos);
		}

		public async Task<IEnumerable<ResumoPedidoDTO>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
		{
			var pedidos = await _unitOfWork.Pedidos.ObterPorPeriodoAsync(dataInicio, dataFim);
			return _mapeador.Map<IEnumerable<ResumoPedidoDTO>>(pedidos);
		}

		public async Task<PedidoDTO> CriarAsync(CriarPedidoDTO dto)
		{
			await _unitOfWork.BeginTransactionAsync();

			try
			{
				// Verificar cliente
				var cliente = await _unitOfWork.Clientes.ObterPorIdAsync(dto.ClienteId);
				if (cliente == null)
					throw new Exception("Cliente não encontrado");

				// Criar pedido
				var pedido = new Pedido
				{
					NumeroPedido = await _unitOfWork.Pedidos.GerarNumeroPedidoAsync(),
					DataPedido = DateTime.UtcNow,
					Status = StatusPedido.Pendente,
					ClienteId = dto.ClienteId,
					EscolaId = dto.EscolaId,
					ValorFrete = dto.ValorFrete,
					ValorDesconto = dto.ValorDesconto,
					PrazoEntregaDias = dto.PrazoEntregaDias,
					Observacoes = dto.Observacoes,
					NomeEntrega = dto.NomeEntrega,
					TelefoneEntrega = dto.TelefoneEntrega,
					CepEntrega = dto.CepEntrega,
					LogradouroEntrega = dto.LogradouroEntrega,
					NumeroEntrega = dto.NumeroEntrega,
					ComplementoEntrega = dto.ComplementoEntrega,
					BairroEntrega = dto.BairroEntrega,
					CidadeEntrega = dto.CidadeEntrega,
					EstadoEntrega = dto.EstadoEntrega,
					Ativo = true,
					DataCriacao = DateTime.UtcNow
				};

				// Adicionar itens
				decimal valorSubtotal = 0;
				foreach (var itemDto in dto.Itens)
				{
					var produto = await _unitOfWork.Produtos.ObterPorIdAsync(itemDto.ProdutoId);
					if (produto == null)
						throw new Exception($"Produto {itemDto.ProdutoId} não encontrado");

					if (!itemDto.ProdutoTamanhoId.HasValue)
						throw new Exception($"Produto {produto.Nome} precisa de um tamanho informado");

					var tamanho = await _unitOfWork.ProdutoTamanhos.ObterPorIdAsync(itemDto.ProdutoTamanhoId.Value);
					if (tamanho == null)
						throw new Exception($"Tamanho não encontrado para o produto {produto.Nome}");

					if (tamanho.QuantidadeEstoque < itemDto.Quantidade)
						throw new Exception($"Estoque insuficiente para o produto {produto.Nome}");

					var precoUnitario = produto.PrecoPromocional ?? produto.Preco;
					var item = new ItemPedido
					{
						ProdutoId = itemDto.ProdutoId,
						ProdutoTamanhoId = itemDto.ProdutoTamanhoId,
						Quantidade = itemDto.Quantidade,
						PrecoUnitario = precoUnitario,
						ValorTotal = precoUnitario * itemDto.Quantidade,
						Observacoes = itemDto.Observacoes,
						Ativo = true,
						DataCriacao = DateTime.UtcNow
					};

					pedido.Itens.Add(item);
					valorSubtotal += item.ValorTotal;

					// Atualizar estoque do tamanho
					var estoqueAnterior = tamanho.QuantidadeEstoque;
					tamanho.QuantidadeEstoque -= itemDto.Quantidade;
					tamanho.DataAtualizacao = DateTime.UtcNow;
					await _unitOfWork.ProdutoTamanhos.AtualizarAsync(tamanho);

					// Registrar movimentação de estoque (tamanho)
					var movimentacao = new MovimentacaoEstoque
					{
						ProdutoTamanhoId = tamanho.Id,
						Quantidade = itemDto.Quantidade,
						Tipo = TipoMovimentacao.Saida,
						Motivo = $"Venda - Pedido {pedido.NumeroPedido} - Tamanho {tamanho.Tamanho}",
						DataMovimentacao = DateTime.UtcNow,
						EstoqueAnterior = estoqueAnterior,
						EstoqueAtual = tamanho.QuantidadeEstoque,
						Referencia = pedido.NumeroPedido,
						Ativo = true
					};
					await _unitOfWork.MovimentacoesEstoque.AdicionarAsync(movimentacao);
				}

				pedido.ValorSubtotal = valorSubtotal;
				pedido.ValorTotal = valorSubtotal + dto.ValorFrete - dto.ValorDesconto;

				await _unitOfWork.Pedidos.AdicionarAsync(pedido);
				await _unitOfWork.SalvarMudancasAsync();
				await _unitOfWork.CommitTransactionAsync();

				// Enviar notificações
				await _NotificacaoService.EnviarEmailNovaVendaAsync(pedido);
				await _NotificacaoService.EnviarWhatsAppNovaVendaAsync(pedido);
				await _NotificacaoService.EnviarEmailClienteNovaVendaAsync(pedido);

				// Verificar estoque baixo
				foreach (var item in pedido.Itens)
				{
					var produto = await _unitOfWork.Produtos.ObterPorIdAsync(item.ProdutoId);
					if (produto == null)
						continue;

					var tamanhos = await _unitOfWork.ProdutoTamanhos.ObterTodosPorProduto(produto.Id);
					var estoqueTotal = tamanhos.Sum(t => t.QuantidadeEstoque);
					if (estoqueTotal <= produto.QuantidadeMinimaEstoque)
					{
						await _NotificacaoService.EnviarEmailEstoqueBaixoAsync(produto, estoqueTotal);
					}
				}

				return _mapeador.Map<PedidoDTO>(pedido);
			}
			catch (Exception)
			{
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<PedidoDTO?> AtualizarStatusAsync(AtualizarStatusPedidoDTO dto)
		{
			var pedido = await _unitOfWork.Pedidos.ObterComItensAsync(dto.Id);
			if (pedido == null)
				return null;

			var statusAnterior = pedido.Status;
			if (statusAnterior == StatusPedido.Pago &&
				(dto.Status == StatusPedido.Pendente || dto.Status == StatusPedido.AguardandoPagamento))
			{
				throw new Exception("Não é permitido retornar um pedido pago para Pendente ou Aguardando Pagamento.");
			}

			pedido.Status = dto.Status;
			pedido.DataAtualizacao = DateTime.UtcNow;

			if (statusAnterior != dto.Status && dto.Status == StatusPedido.Cancelado)
			{
				await ProcessarCancelamentoAsync(pedido);
			}

			await _unitOfWork.Pedidos.AtualizarAsync(pedido);
			await _unitOfWork.SalvarMudancasAsync();

			// Enviar email ao cliente notificando a mudança de status
			if (statusAnterior != dto.Status)
			{
				await _NotificacaoService.EnviarEmailAlteracaoStatusAsync(pedido);
			}

			return _mapeador.Map<PedidoDTO>(pedido);
		}

		private async Task ProcessarCancelamentoAsync(Pedido pedido)
		{
			// Repor estoque dos itens e registrar movimentação de devolução
			foreach (var item in pedido.Itens)
			{
				if (!item.ProdutoTamanhoId.HasValue)
					continue;

				var tamanho = await _unitOfWork.ProdutoTamanhos.ObterPorIdAsync(item.ProdutoTamanhoId.Value);
				if (tamanho == null)
					continue;

				var estoqueAnterior = tamanho.QuantidadeEstoque;
				tamanho.QuantidadeEstoque += item.Quantidade;
				tamanho.DataAtualizacao = DateTime.UtcNow;
				await _unitOfWork.ProdutoTamanhos.AtualizarAsync(tamanho);

				var movimentacao = new MovimentacaoEstoque
				{
					ProdutoTamanhoId = tamanho.Id,
					Quantidade = item.Quantidade,
					Tipo = TipoMovimentacao.Devolucao,
					Motivo = $"Cancelamento - Pedido {pedido.NumeroPedido} - Tamanho {tamanho.Tamanho}",
					DataMovimentacao = DateTime.UtcNow,
					EstoqueAnterior = estoqueAnterior,
					EstoqueAtual = tamanho.QuantidadeEstoque,
					Referencia = pedido.NumeroPedido,
					Ativo = true
				};
				await _unitOfWork.MovimentacoesEstoque.AdicionarAsync(movimentacao);
			}

			// Desativar movimentações de saída ligadas ao pedido
			var movimentacoes = await _unitOfWork.MovimentacoesEstoque.ObterPorReferenciaAsync(pedido.NumeroPedido);
			foreach (var mov in movimentacoes.Where(m => m.Tipo == TipoMovimentacao.Saida))
			{
				mov.Ativo = false;
				mov.DataAtualizacao = DateTime.UtcNow;
				await _unitOfWork.MovimentacoesEstoque.AtualizarAsync(mov);
			}

			// Notificar administrador
			await _NotificacaoService.EnviarEmailCancelamentoPedidoAsync(pedido);
		}

		public async Task<PedidoDTO?> AtualizarNotaFiscalAsync(AtualizarNotaFiscalDTO dto)
		{
			var pedido = await _unitOfWork.Pedidos.ObterComItensAsync(dto.Id);
			if (pedido == null)
				return null;

			pedido.NotaFiscalUrl = dto.NotaFiscalUrl;
			pedido.DataAtualizacao = DateTime.UtcNow;

			await _unitOfWork.Pedidos.AtualizarAsync(pedido);
			await _unitOfWork.SalvarMudancasAsync();

			// Enviar email ao cliente com a nota fiscal
			if (!string.IsNullOrEmpty(dto.NotaFiscalUrl))
			{
				await _NotificacaoService.EnviarEmailNotaFiscalAsync(pedido);
			}

			return _mapeador.Map<PedidoDTO>(pedido);
		}
	}
}
