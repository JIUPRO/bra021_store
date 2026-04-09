using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Enums;
using LojaVirtual.Dominio.Interfaces;
using Mapster;
using Microsoft.Extensions.Logging;

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
		private readonly INotificacaoService _NotificacaoService;
		private readonly IFreteService _freteService;
		private readonly ILogger<PedidoService> _logger;

		public PedidoService(IUnitOfWork unitOfWork, INotificacaoService notificacaoService, IFreteService freteService, ILogger<PedidoService> logger)
		{
			_unitOfWork = unitOfWork;
			_NotificacaoService = notificacaoService;
			_freteService = freteService;
			_logger = logger;
		}

		public async Task<IEnumerable<ResumoPedidoDTO>> ObterTodosAsync()
		{
			var pedidos = await _unitOfWork.Pedidos.ObterTodosAsync();
			return pedidos.Adapt<IEnumerable<ResumoPedidoDTO>>();
		}

		public async Task<PedidoDTO?> ObterPorIdAsync(Guid id)
		{
			var pedido = await _unitOfWork.Pedidos.ObterComItensAsync(id);
			return pedido?.Adapt<PedidoDTO>();
		}

		public async Task<IEnumerable<ResumoPedidoDTO>> ObterPorClienteAsync(Guid clienteId)
		{
			var pedidos = await _unitOfWork.Pedidos.ObterPorClienteAsync(clienteId);
			return pedidos.Adapt<IEnumerable<ResumoPedidoDTO>>();
		}

		public async Task<IEnumerable<ResumoPedidoDTO>> ObterPorStatusAsync(StatusPedido status)
		{
			var pedidos = await _unitOfWork.Pedidos.ObterPorStatusAsync(status);
			return pedidos.Adapt<IEnumerable<ResumoPedidoDTO>>();
		}

		public async Task<IEnumerable<ResumoPedidoDTO>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
		{
			var pedidos = await _unitOfWork.Pedidos.ObterPorPeriodoAsync(dataInicio, dataFim);
			return pedidos.Adapt<IEnumerable<ResumoPedidoDTO>>();
		}

		public async Task<PedidoDTO> CriarAsync(CriarPedidoDTO dto)
		{
			await _unitOfWork.BeginTransactionAsync();

			try
			{
				var cliente = await _unitOfWork.Clientes.ObterPorIdAsync(dto.ClienteId);
				if (cliente == null)
				{
					throw new Exception("Cliente não encontrado");
				}

				LojaVirtual.Dominio.Entidades.Escola? escola = null;
				if (dto.EscolaId.HasValue)
				{
					escola = await _unitOfWork.Escolas.ObterPorIdAsync(dto.EscolaId.Value);
				}

				var cotacaoFrete = await _freteService.CotarAsync(new CotacaoFreteRequestDTO
				{
					CepDestino = dto.CepEntrega,
					CodigoServico = EhProviderDinamico(dto.ProviderFrete)
						? dto.CodigoServicoFrete
						: null,
					Itens = dto.Itens.Select(item => new CotacaoFreteItemDTO
					{
						ProdutoId = item.ProdutoId,
						ProdutoTamanhoId = item.ProdutoTamanhoId,
						Quantidade = item.Quantidade
					}).ToList()
				});
				var opcaoFrete = ObterOpcaoFreteFinal(dto, cotacaoFrete);

				var pedido = new Pedido
				{
					NumeroPedido = await _unitOfWork.Pedidos.GerarNumeroPedidoAsync(),
					DataPedido = DateTime.UtcNow,
					Status = StatusPedido.Pendente,
					ClienteId = dto.ClienteId,
					EscolaId = dto.EscolaId,
					Cliente = cliente,
					Escola = escola,
					ValorFrete = opcaoFrete.Valor,
					ValorDesconto = dto.ValorDesconto,
					PrazoPreparacaoDias = opcaoFrete.PrazoPreparacaoDias,
					PrazoEnvioDias = opcaoFrete.PrazoEnvioDias,
					PrazoEntregaDias = opcaoFrete.PrazoEntregaDias,
					Observacoes = dto.Observacoes,
					TipoEntrega = dto.TipoEntrega,
					TransportadoraFrete = dto.TransportadoraFrete ?? opcaoFrete.NomeTransportadora,
					ServicoFrete = dto.NomeServicoFrete ?? opcaoFrete.NomeServico,
					CodigoServicoFrete = dto.CodigoServicoFrete ?? opcaoFrete.CodigoServico,
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

				decimal valorSubtotal = 0;
				foreach (var itemDto in dto.Itens)
				{
					var produto = await _unitOfWork.Produtos.ObterPorIdAsync(itemDto.ProdutoId);
					if (produto == null)
					{
						throw new Exception($"Produto {itemDto.ProdutoId} não encontrado");
					}

					if (!itemDto.ProdutoTamanhoId.HasValue)
					{
						throw new Exception($"Produto {produto.Nome} precisa de um tamanho informado");
					}

					var tamanho = await _unitOfWork.ProdutoTamanhos.ObterPorIdAsync(itemDto.ProdutoTamanhoId.Value);
					if (tamanho == null)
					{
						throw new Exception($"Tamanho não encontrado para o produto {produto.Nome}");
					}

					if (tamanho.QuantidadeEstoque < itemDto.Quantidade)
					{
						throw new Exception($"Estoque insuficiente para o produto {produto.Nome}");
					}

					var precoUnitario = produto.PrecoPromocional ?? produto.Preco;
					var item = new ItemPedido
					{
						ProdutoId = itemDto.ProdutoId,
						Produto = produto,
						ProdutoTamanhoId = itemDto.ProdutoTamanhoId,
						ProdutoTamanho = tamanho,
						Quantidade = itemDto.Quantidade,
						PrecoUnitario = precoUnitario,
						ValorTotal = precoUnitario * itemDto.Quantidade,
						Observacoes = itemDto.Observacoes,
						Ativo = true,
						DataCriacao = DateTime.UtcNow
					};

					pedido.Itens.Add(item);
					valorSubtotal += item.ValorTotal;

					var estoqueAnterior = tamanho.QuantidadeEstoque;
					tamanho.QuantidadeEstoque -= itemDto.Quantidade;
					tamanho.DataAtualizacao = DateTime.UtcNow;
					await _unitOfWork.ProdutoTamanhos.AtualizarAsync(tamanho);

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
				pedido.ValorTotal = valorSubtotal + pedido.ValorFrete - dto.ValorDesconto;

				await _unitOfWork.Pedidos.AdicionarAsync(pedido);
				await _unitOfWork.SalvarMudancasAsync();
				await _unitOfWork.CommitTransactionAsync();

				await _NotificacaoService.EnviarEmailNovaVendaAsync(pedido);
				await _NotificacaoService.EnviarWhatsAppNovaVendaAsync(pedido);
				await _NotificacaoService.EnviarEmailClienteNovaVendaAsync(pedido);

				foreach (var item in pedido.Itens)
				{
					var produto = await _unitOfWork.Produtos.ObterPorIdAsync(item.ProdutoId);
					if (produto == null)
					{
						continue;
					}

					var tamanhos = await _unitOfWork.ProdutoTamanhos.ObterTodosPorProduto(produto.Id);
					var estoqueTotal = tamanhos.Sum(t => t.QuantidadeEstoque);
					if (estoqueTotal <= produto.QuantidadeMinimaEstoque)
					{
						await _NotificacaoService.EnviarEmailEstoqueBaixoAsync(produto, estoqueTotal);
					}
				}

				return pedido.Adapt<PedidoDTO>();
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
			{
				return null;
			}

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

			if (statusAnterior != dto.Status)
			{
				await _NotificacaoService.EnviarEmailAlteracaoStatusAsync(pedido);
			}

			return pedido.Adapt<PedidoDTO>();
		}

		private async Task ProcessarCancelamentoAsync(Pedido pedido)
		{
			foreach (var item in pedido.Itens)
			{
				if (!item.ProdutoTamanhoId.HasValue)
				{
					continue;
				}

				var tamanho = await _unitOfWork.ProdutoTamanhos.ObterPorIdAsync(item.ProdutoTamanhoId.Value);
				if (tamanho == null)
				{
					continue;
				}

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

			var movimentacoes = await _unitOfWork.MovimentacoesEstoque.ObterPorReferenciaAsync(pedido.NumeroPedido);
			foreach (var mov in movimentacoes.Where(m => m.Tipo == TipoMovimentacao.Saida))
			{
				mov.Ativo = false;
				mov.DataAtualizacao = DateTime.UtcNow;
				await _unitOfWork.MovimentacoesEstoque.AtualizarAsync(mov);
			}

			await _NotificacaoService.EnviarEmailCancelamentoPedidoAsync(pedido);
		}

		public async Task<PedidoDTO?> AtualizarNotaFiscalAsync(AtualizarNotaFiscalDTO dto)
		{
			var pedido = await _unitOfWork.Pedidos.ObterComItensAsync(dto.Id);
			if (pedido == null)
			{
				return null;
			}

			pedido.NotaFiscalUrl = dto.NotaFiscalUrl;
			pedido.NotaFiscalKey = dto.NotaFiscalKey;
			pedido.DataAtualizacao = DateTime.UtcNow;

			await _unitOfWork.Pedidos.AtualizarAsync(pedido);
			await _unitOfWork.SalvarMudancasAsync();

			if (!string.IsNullOrEmpty(dto.NotaFiscalUrl))
			{
				await _NotificacaoService.EnviarEmailNotaFiscalAsync(pedido);
			}

			return pedido.Adapt<PedidoDTO>();
		}

		private OpcaoFreteDTO ObterOpcaoFreteFinal(CriarPedidoDTO dto, CotacaoFreteResponseDTO cotacaoFrete)
		{
			var providerDinamicoSelecionado = EhProviderDinamico(dto.ProviderFrete);
			var cotacaoDinamicaDisponivel = EhProviderDinamico(cotacaoFrete.ProviderUtilizado) &&
				string.Equals(cotacaoFrete.ProviderUtilizado, dto.ProviderFrete, StringComparison.OrdinalIgnoreCase) &&
				!cotacaoFrete.UsandoFallbackFixo;

			if (providerDinamicoSelecionado && cotacaoDinamicaDisponivel)
			{
				return _freteService.SelecionarOpcao(cotacaoFrete, dto.CodigoServicoFrete);
			}

			if (providerDinamicoSelecionado && dto.ValorFrete > 0 && dto.PrazoEntregaDias > 0)
			{
				_logger.LogWarning(
					"Pedido usando cotação escolhida no checkout como fallback. Pedido cliente {ClienteId}, serviço {CodigoServicoFrete}, valor {ValorFrete}, prazo {PrazoEntregaDias}. Mensagem cotação: {Mensagem}",
					dto.ClienteId,
					dto.CodigoServicoFrete,
					dto.ValorFrete,
					dto.PrazoEntregaDias,
					cotacaoFrete.Mensagem);

				return new OpcaoFreteDTO
				{
					Provider = dto.ProviderFrete ?? "Dinamico",
					CodigoServico = dto.CodigoServicoFrete,
					NomeServico = dto.NomeServicoFrete ?? "Frete dinâmico",
					NomeTransportadora = dto.TransportadoraFrete,
					Valor = dto.ValorFrete,
					PrazoPreparacaoDias = dto.PrazoPreparacaoDias,
					PrazoEnvioDias = dto.PrazoEnvioDias,
					PrazoEntregaDias = dto.PrazoEntregaDias
				};
			}

			return _freteService.SelecionarOpcao(cotacaoFrete, dto.CodigoServicoFrete);
		}

		private static bool EhProviderDinamico(string? provider)
		{
			return !string.IsNullOrWhiteSpace(provider) &&
				!string.Equals(provider, "Fixo", StringComparison.OrdinalIgnoreCase);
		}
	}
}
