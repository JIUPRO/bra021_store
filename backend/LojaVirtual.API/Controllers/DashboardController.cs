using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Dominio.Enums;
using LojaVirtual.Infraestrutura.Data;

namespace LojaVirtual.API.Controllers
{
	[ApiController]
	[Route("api/dashboard")]
	[Authorize]
	public class DashboardController : ControllerBase
	{
		private readonly LojaDbContext _dbContext;

		public DashboardController(LojaDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		[HttpGet]
		public async Task<ActionResult<DashboardDTO>> ObterDashboard()
		{
			var agora = DateTime.UtcNow;
			var inicioMes = new DateTime(agora.Year, agora.Month, 1);
			var proximoMes = inicioMes.AddMonths(1);

			var totalPedidos = await _dbContext.Pedidos.CountAsync();
			var vendasMes = await _dbContext.Pedidos
				.Where(p => p.DataPedido >= inicioMes && p.DataPedido < proximoMes)
				.SumAsync(p => (decimal?)p.ValorTotal) ?? 0m;

			var totalProdutos = await _dbContext.Produtos.CountAsync(p => p.Ativo);
			var totalClientes = await _dbContext.Clientes.CountAsync(c => c.Ativo);

			var pedidosRecentes = await _dbContext.Pedidos
				.AsNoTracking()
				.Include(p => p.Cliente)
				.OrderByDescending(p => p.DataPedido)
				.Take(5)
				.Select(p => new DashboardPedidoRecenteDTO
				{
					Id = p.Id,
					NumeroPedido = p.NumeroPedido,
					NomeCliente = p.Cliente.Nome,
					DataPedido = p.DataPedido,
					Status = p.Status,
					StatusDescricao = ObterDescricaoStatus(p.Status),
					ValorTotal = p.ValorTotal
				})
				.ToListAsync();

			var estoqueBaixo = await _dbContext.Produtos
				.AsNoTracking()
				.Where(p => p.Ativo)
				.Select(p => new
				{
					Produto = p,
					Quantidade = p.Tamanhos.Sum(t => t.QuantidadeEstoque)
				})
				.Where(x => x.Quantidade <= x.Produto.QuantidadeMinimaEstoque)
				.OrderBy(x => x.Quantidade)
				.Take(5)
				.Select(x => new DashboardEstoqueBaixoDTO
				{
					Produto = x.Produto.Nome,
					
					Quantidade = x.Quantidade
				})
				.ToListAsync();

			var alertas = new List<DashboardAlertaDTO>();

			var qtdEstoqueBaixo = await _dbContext.Produtos
				.AsNoTracking()
				.Where(p => p.Ativo)
				.Select(p => new
				{
					Produto = p,
					Quantidade = p.Tamanhos.Sum(t => t.QuantidadeEstoque)
				})
				.CountAsync(x => x.Quantidade <= x.Produto.QuantidadeMinimaEstoque);
			if (qtdEstoqueBaixo > 0)
			{
				alertas.Add(new DashboardAlertaDTO
				{
					Titulo = "Estoque Baixo",
					Mensagem = $"{qtdEstoqueBaixo} produto(s) estão com estoque abaixo do mínimo"
				});
			}

			var qtdPendentes = await _dbContext.Pedidos
				.AsNoTracking()
				.CountAsync(p => p.Status == StatusPedido.Pendente || p.Status == StatusPedido.AguardandoPagamento);
			if (qtdPendentes > 0)
			{
				alertas.Add(new DashboardAlertaDTO
				{
					Titulo = "Pedidos Pendentes",
					Mensagem = $"{qtdPendentes} pedido(s) aguardando aprovação"
				});
			}

			var dashboard = new DashboardDTO
			{
				Estatisticas = new DashboardEstatisticasDTO
				{
					TotalPedidos = totalPedidos,
					VendasMes = vendasMes,
					TotalProdutos = totalProdutos,
					TotalClientes = totalClientes
				},
				PedidosRecentes = pedidosRecentes,
				Alertas = alertas,
				EstoqueBaixo = estoqueBaixo
			};

			return Ok(dashboard);
		}

		private static string ObterDescricaoStatus(StatusPedido status)
		{
			return status switch
			{
				StatusPedido.Pendente => "Pendente",
				StatusPedido.AguardandoPagamento => "Aguardando Pagamento",
				StatusPedido.Pago => "Pago",
				StatusPedido.EmSeparacao => "Em Separação",
				StatusPedido.Enviado => "Enviado",
				StatusPedido.Entregue => "Entregue",
				StatusPedido.Cancelado => "Cancelado",
				_ => status.ToString()
			};
		}
	}
}
