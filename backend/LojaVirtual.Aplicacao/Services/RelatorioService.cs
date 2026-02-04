using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Enums;

namespace LojaVirtual.Aplicacao.Services
{
	public class RelatorioService
	{
		public RelatorioService()
		{
			QuestPDF.Settings.License = LicenseType.Community;
		}

		public byte[] GerarRelatorioPedidos(List<Pedido> pedidos, DateTime dataInicio, DateTime dataFim)
		{
			var document = Document.Create(container =>
			{
				container.Page(page =>
				{
					page.Margin(20);
					page.DefaultTextStyle(x => x.FontSize(8));

					page.Header().Element(ComposeHeader);

					page.Content().Column(column =>
					{
						column.Item().PaddingVertical(10).Column(col =>
						{
							col.Item().Text("RELATÓRIO DE PEDIDOS")
								  .FontSize(14)
								  .Bold()
								  .AlignCenter();

							col.Item().Text($"Período: {dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}")
								  .FontSize(8)
								  .AlignCenter()
								  .FontColor("#666");
						});

						column.Item().Row(row =>
						{
							row.RelativeItem().Column(col =>
							{
								col.Item().Text("Total de Pedidos: ")
										.Bold()
										.FontSize(8);
								col.Item().Text(pedidos.Count.ToString())
										.FontSize(9)
										.Bold();
							});

							row.RelativeItem().Column(col =>
							{
								var totalVendas = pedidos.Sum(p => p.Itens?.Sum(i => i.PrecoUnitario * i.Quantidade) ?? 0);
								col.Item().Text("Total de Vendas: ")
										.Bold()
										.FontSize(8);
								col.Item().Text($"R$ {totalVendas:F2}")
										.FontSize(9)
										.Bold()
										.FontColor("#10b981");
							});
						});

						column.Item().PaddingVertical(10).Table(table =>
						{
							table.ColumnsDefinition(columns =>
							{
								columns.RelativeColumn(1.2f);
								columns.RelativeColumn(1.8f);
								columns.RelativeColumn(1.2f);
								columns.RelativeColumn(0.8f);
								columns.RelativeColumn(1.2f);
								columns.RelativeColumn(1.2f);
							});

							table.Header(header =>
							{
								header.Cell().Background("#10b981").Padding(3).Text("Número").Bold().FontSize(7).FontColor("#FFFFFF");
								header.Cell().Background("#10b981").Padding(3).Text("Cliente").Bold().FontSize(7).FontColor("#FFFFFF");
								header.Cell().Background("#10b981").Padding(3).Text("Data").Bold().FontSize(7).FontColor("#FFFFFF");
								header.Cell().Background("#10b981").Padding(3).Text("Itens").Bold().FontSize(7).FontColor("#FFFFFF").AlignCenter();
								header.Cell().Background("#10b981").Padding(3).Text("Total").Bold().FontSize(7).FontColor("#FFFFFF").AlignRight();
								header.Cell().Background("#10b981").Padding(3).Text("Status").Bold().FontSize(7).FontColor("#FFFFFF");
							});

							foreach (var pedido in pedidos)
							{
								var totalPedido = pedido.Itens?.Sum(i => i.PrecoUnitario * i.Quantidade) ?? 0;
								var quantidadeItens = pedido.Itens?.Count ?? 0;
								var statusDescricao = ObterDescricaoStatus(pedido.Status);

								table.Cell().Padding(3).Text($"#{pedido.NumeroPedido}").FontSize(7);
								table.Cell().Padding(3).Text(pedido.Cliente?.Nome ?? "---").FontSize(7);
								table.Cell().Padding(3).Text(pedido.DataPedido.ToString("dd/MM/yyyy")).FontSize(7);
								table.Cell().Padding(3).Text(quantidadeItens.ToString()).FontSize(7).AlignCenter();
								table.Cell().Padding(3).Text($"R$ {totalPedido:F2}").FontSize(7).AlignRight();
								table.Cell().Padding(3).Text(statusDescricao).FontSize(7)
										.FontColor(ObterCorStatus(pedido.Status));
							}
						});
					});

					page.Footer().Element(ComposeFooter);
				});
			});

			return document.GeneratePdf();
		}

		private void ComposeHeader(IContainer container)
		{
			container.Column(column =>
			{
				column.Item().Row(row =>
				{
					row.RelativeItem().Column(col =>
					{
						col.Item().Text("Loja Brazil021 School of Jiu-Jitsu")
								 .Bold()
								 .FontSize(14)
								 .FontColor("#10b981");
						col.Item().Text("Sistema de Gestão")
								 .FontSize(9)
								 .FontColor("#666");
					});

					row.RelativeItem().AlignRight().Column(col =>
					{
						col.Item().Text($"Gerado em {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
								 .FontSize(9)
								 .FontColor("#999");
					});
				});

				column.Item().PaddingVertical(5).BorderBottom(1).BorderColor("#e5e7eb");
			});
		}

		private void ComposeFooter(IContainer container)
		{
			container.PaddingTop(5).BorderTop(1).BorderColor("#e5e7eb").Row(row =>
			{
				row.RelativeItem().Text("Página 1").FontSize(8).FontColor("#999");
				row.RelativeItem().AlignRight().Text("© 2026 Loja Brazil021 - Todos os direitos reservados").FontSize(8).FontColor("#999");
			});
		}

		private string ObterDescricaoStatus(StatusPedido status)
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
				_ => "Desconhecido"
			};
		}

		public byte[] GerarRelatorioComissao(List<Pedido> pedidos, DateTime dataInicio, DateTime dataFim)
		{
			// Agrupar pedidos por Escola
			var comissoesPorEscola = pedidos
				.Where(p => p.Escola != null)
				.GroupBy(p => p.Escola)
				.Select(g => new
				{
					Escola = g.Key,
					TotalVendas = g.Sum(p => p.Itens?.Sum(i => i.PrecoUnitario * i.Quantidade) ?? 0),
					QuantidadePedidos = g.Count(),
					Comissao = g.Sum(p => p.Itens?.Sum(i => i.PrecoUnitario * i.Quantidade) ?? 0) * ((decimal)(g.Key?.PercentualComissao ?? 0) / 100)
				})
				.ToList();

			var document = Document.Create(container =>
			{
				container.Page(page =>
				{
					page.Margin(20);
					page.DefaultTextStyle(x => x.FontSize(8));

					page.Header().Element(ComposeHeader);

					page.Content().Column(column =>
					{
						column.Item().PaddingVertical(10).Column(col =>
						{
							col.Item().Text("RELATÓRIO DE COMISSÕES")
								  .FontSize(14)
								  .Bold()
								  .AlignCenter();

							col.Item().Text($"Período: {dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}")
								  .FontSize(8)
								  .AlignCenter()
								  .FontColor("#666");
						});

						// Verificar se há dados
						if (comissoesPorEscola.Count == 0)
						{
							column.Item().PaddingVertical(40).Text("Nenhum pedido com escola associada encontrado no período.")
								.FontSize(10)
								.AlignCenter()
								.FontColor("#666");
							return;
						}

						column.Item().Row(row =>
						{
							row.RelativeItem().Column(col =>
							{
								col.Item().Text("Total de Escolas: ")
										.Bold()
										.FontSize(8);
								col.Item().Text(comissoesPorEscola.Count.ToString())
										.FontSize(9)
										.Bold();
							});

							row.RelativeItem().Column(col =>
							{
								var totalVendas = comissoesPorEscola.Sum(c => c.TotalVendas);
								col.Item().Text("Total de Vendas: ")
										.Bold()
										.FontSize(8);
								col.Item().Text($"R$ {totalVendas:F2}")
										.FontSize(9)
										.Bold()
										.FontColor("#10b981");
							});

							row.RelativeItem().Column(col =>
							{
								var totalComissao = comissoesPorEscola.Sum(c => c.Comissao);
								col.Item().Text("Total de Comissões: ")
										.Bold()
										.FontSize(8);
								col.Item().Text($"R$ {totalComissao:F2}")
										.FontSize(9)
										.Bold()
										.FontColor("#10b981");
							});
						});

						column.Item().PaddingVertical(10).Table(table =>
						{
							table.ColumnsDefinition(columns =>
							{
								columns.RelativeColumn(2f);
								columns.RelativeColumn(1.2f);
								columns.RelativeColumn(1.2f);
								columns.RelativeColumn(1f);
								columns.RelativeColumn(1.2f);
								columns.RelativeColumn(1.2f);
							});

							table.Header(header =>
							{
								header.Cell().Background("#10b981").Padding(3).Text("Escola").Bold().FontSize(7).FontColor("#FFFFFF");
								header.Cell().Background("#10b981").Padding(3).Text("Professor").Bold().FontSize(7).FontColor("#FFFFFF");
								header.Cell().Background("#10b981").Padding(3).Text("Pedidos").Bold().FontSize(7).FontColor("#FFFFFF").AlignCenter();
								header.Cell().Background("#10b981").Padding(3).Text("% Comissão").Bold().FontSize(7).FontColor("#FFFFFF").AlignCenter();
								header.Cell().Background("#10b981").Padding(3).Text("Total Vendas").Bold().FontSize(7).FontColor("#FFFFFF").AlignRight();
								header.Cell().Background("#10b981").Padding(3).Text("Comissão").Bold().FontSize(7).FontColor("#FFFFFF").AlignRight();
							});

							foreach (var comissao in comissoesPorEscola.OrderByDescending(c => c.Comissao))
							{
								table.Cell().Padding(3).Text(comissao.Escola?.Nome ?? "---").FontSize(7);
								table.Cell().Padding(3).Text(comissao.Escola?.ProfessorResponsavel ?? "---").FontSize(7);
								table.Cell().Padding(3).Text(comissao.QuantidadePedidos.ToString()).FontSize(7).AlignCenter();
								table.Cell().Padding(3).Text($"{comissao.Escola?.PercentualComissao:F2}%").FontSize(7).AlignCenter();
								table.Cell().Padding(3).Text($"R$ {comissao.TotalVendas:F2}").FontSize(7).AlignRight();
								table.Cell().Padding(3).Text($"R$ {comissao.Comissao:F2}").FontSize(7).AlignRight().Bold();
							}
						});
					});

					page.Footer().Element(ComposeFooter);
				});
			});

			return document.GeneratePdf();
		}

		private string ObterCorStatus(StatusPedido status)
		{
			return status switch
			{
				StatusPedido.Pendente => "#f59e0b",
				StatusPedido.AguardandoPagamento => "#f59e0b",
				StatusPedido.Pago => "#3b82f6",
				StatusPedido.EmSeparacao => "#8b5cf6",
				StatusPedido.Enviado => "#06b6d4",
				StatusPedido.Entregue => "#10b981",
				StatusPedido.Cancelado => "#ef4444",
				_ => "#6b7280"
			};
		}

		public byte[] GerarRelatorioProdutosMaisVendidos(List<Pedido> pedidos, DateTime dataInicio, DateTime dataFim)
		{
			// Agrupar itens por produto e contar quantidade/vendas
			var produtosMaisVendidos = pedidos
				.SelectMany(p => p.Itens ?? new List<ItemPedido>())
				.GroupBy(i => i.Produto)
				.Select(g => new
				{
					Produto = g.Key,
					QuantidadeVendida = g.Sum(i => i.Quantidade),
					TotalVendas = g.Sum(i => i.PrecoUnitario * i.Quantidade)
				})
				.OrderByDescending(x => x.QuantidadeVendida)
				.Take(20)
				.ToList();

			var document = Document.Create(container =>
			{
				container.Page(page =>
				{
					page.Margin(20);
					page.DefaultTextStyle(x => x.FontSize(8));
					page.Header().Element(ComposeHeader);

					page.Content().Column(column =>
					{
						column.Item().PaddingVertical(10).Column(col =>
						{
							col.Item().Text("RELATÓRIO DE PRODUTOS MAIS VENDIDOS")
								  .FontSize(14).Bold().AlignCenter();
							col.Item().Text($"Período: {dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}")
								  .FontSize(8).AlignCenter().FontColor("#666");
						});

						// Verificar se há dados
						if (produtosMaisVendidos.Count == 0)
						{
							column.Item().PaddingVertical(40).Text("Nenhum produto vendido encontrado no período.")
								.FontSize(10)
								.AlignCenter()
								.FontColor("#666");
							return;
						}

						column.Item().Row(row =>
						{
							row.RelativeItem().Column(col =>
							{
								col.Item().Text("Total de Produtos: ").Bold().FontSize(8);
								col.Item().Text(produtosMaisVendidos.Count.ToString()).FontSize(9).Bold();
							});

							row.RelativeItem().Column(col =>
							{
								var totalVendas = produtosMaisVendidos.Sum(p => p.TotalVendas);
								col.Item().Text("Total de Vendas: ").Bold().FontSize(8);
								col.Item().Text($"R$ {totalVendas:F2}").FontSize(9).Bold().FontColor("#10b981");
							});
						});

						column.Item().PaddingVertical(10).Table(table =>
						{
							table.ColumnsDefinition(columns =>
							{
								columns.RelativeColumn(2.5f);
								columns.RelativeColumn(1.2f);
								columns.RelativeColumn(1.5f);
							});

							table.Header(header =>
							{
								header.Cell().Background("#10b981").Padding(3).Text("Produto").Bold().FontSize(7).FontColor("#FFFFFF");
								header.Cell().Background("#10b981").Padding(3).Text("Quantidade").Bold().FontSize(7).FontColor("#FFFFFF").AlignCenter();
								header.Cell().Background("#10b981").Padding(3).Text("Total de Vendas").Bold().FontSize(7).FontColor("#FFFFFF").AlignRight();
							});

							foreach (var produto in produtosMaisVendidos)
							{
								table.Cell().Padding(3).Text(produto.Produto?.Nome ?? "---").FontSize(7);
								table.Cell().Padding(3).Text(produto.QuantidadeVendida.ToString()).FontSize(7).AlignCenter();
								table.Cell().Padding(3).Text($"R$ {produto.TotalVendas:F2}").FontSize(7).AlignRight();
							}
						});
					});

					page.Footer().Element(ComposeFooter);
				});
			});

			return document.GeneratePdf();
		}

		public byte[] GerarRelatorioClientes(List<Pedido> pedidos, DateTime dataInicio, DateTime dataFim)
		{
			// Agrupar por cliente
			var clientesComPedidos = pedidos
				.GroupBy(p => p.Cliente)
				.Select(g => new
				{
					Cliente = g.Key,
					QuantidadePedidos = g.Count(),
					TotalGasto = g.Sum(p => p.Itens?.Sum(i => i.PrecoUnitario * i.Quantidade) ?? 0)
				})
				.OrderByDescending(x => x.TotalGasto)
				.ToList();

			var document = Document.Create(container =>
			{
				container.Page(page =>
				{
					page.Margin(20);
					page.DefaultTextStyle(x => x.FontSize(8));
					page.Header().Element(ComposeHeader);

					page.Content().Column(column =>
					{
						column.Item().PaddingVertical(10).Column(col =>
						{
							col.Item().Text("RELATÓRIO DE CLIENTES")
								  .FontSize(14).Bold().AlignCenter();
							col.Item().Text($"Período: {dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}")
								  .FontSize(8).AlignCenter().FontColor("#666");
						});

						column.Item().Row(row =>
						{
							row.RelativeItem().Column(col =>
							{
								col.Item().Text("Total de Clientes: ").Bold().FontSize(8);
								col.Item().Text(clientesComPedidos.Count.ToString()).FontSize(9).Bold();
							});

							row.RelativeItem().Column(col =>
							{
								var totalVendas = clientesComPedidos.Sum(c => c.TotalGasto);
								col.Item().Text("Total de Vendas: ").Bold().FontSize(8);
								col.Item().Text($"R$ {totalVendas:F2}").FontSize(9).Bold().FontColor("#10b981");
							});
						});

						column.Item().PaddingVertical(10).Table(table =>
						{
							table.ColumnsDefinition(columns =>
							{
								columns.RelativeColumn(2f);
								columns.RelativeColumn(1f);
								columns.RelativeColumn(1.5f);
								columns.RelativeColumn(1.2f);
							});

							table.Header(header =>
							{
								header.Cell().Background("#10b981").Padding(3).Text("Cliente").Bold().FontSize(7).FontColor("#FFFFFF");
								header.Cell().Background("#10b981").Padding(3).Text("Pedidos").Bold().FontSize(7).FontColor("#FFFFFF").AlignCenter();
								header.Cell().Background("#10b981").Padding(3).Text("Total Gasto").Bold().FontSize(7).FontColor("#FFFFFF").AlignRight();
								header.Cell().Background("#10b981").Padding(3).Text("Ticket Médio").Bold().FontSize(7).FontColor("#FFFFFF").AlignRight();
							});

							foreach (var cliente in clientesComPedidos)
							{
								var ticketMedio = cliente.TotalGasto / cliente.QuantidadePedidos;
								table.Cell().Padding(3).Text(cliente.Cliente?.Nome ?? "---").FontSize(7);
								table.Cell().Padding(3).Text(cliente.QuantidadePedidos.ToString()).FontSize(7).AlignCenter();
								table.Cell().Padding(3).Text($"R$ {cliente.TotalGasto:F2}").FontSize(7).AlignRight();
								table.Cell().Padding(3).Text($"R$ {ticketMedio:F2}").FontSize(7).AlignRight();
							}
						});
					});

					page.Footer().Element(ComposeFooter);
				});
			});

			return document.GeneratePdf();
		}

		public byte[] GerarRelatorioEstoque(List<Pedido> pedidos, DateTime dataInicio, DateTime dataFim)
		{
			// Agrupar movimentações por produto
			var movimentacoesPorProduto = pedidos
				.SelectMany(p => p.Itens ?? new List<ItemPedido>())
				.GroupBy(i => i.Produto)
				.Select(g => new
				{
					Produto = g.Key,
					QuantidadeSaida = g.Sum(i => i.Quantidade),
					TotalSaida = g.Sum(i => i.PrecoUnitario * i.Quantidade),
					PrecoMedio = g.Average(i => i.PrecoUnitario)
				})
				.OrderByDescending(x => x.QuantidadeSaida)
				.ToList();

			var document = Document.Create(container =>
			{
				container.Page(page =>
				{
					page.Margin(20);
					page.DefaultTextStyle(x => x.FontSize(8));
					page.Header().Element(ComposeHeader);

					page.Content().Column(column =>
					{
						column.Item().PaddingVertical(10).Column(col =>
						{
							col.Item().Text("RELATÓRIO DE MOVIMENTAÇÃO DE ESTOQUE")
								  .FontSize(14).Bold().AlignCenter();
							col.Item().Text($"Período: {dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}")
								  .FontSize(8).AlignCenter().FontColor("#666");
						});

						// Verificar se há dados
						if (movimentacoesPorProduto.Count == 0)
						{
							column.Item().PaddingVertical(40).Text("Nenhuma movimentação de estoque encontrada no período.")
								.FontSize(10)
								.AlignCenter()
								.FontColor("#666");
							return;
						}

						column.Item().Row(row =>
						{
							row.RelativeItem().Column(col =>
							{
								col.Item().Text("Produtos com Saída: ").Bold().FontSize(8);
								col.Item().Text(movimentacoesPorProduto.Count.ToString()).FontSize(9).Bold();
							});

							row.RelativeItem().Column(col =>
							{
								var totalSaida = movimentacoesPorProduto.Sum(p => p.TotalSaida);
								col.Item().Text("Total de Saída: ").Bold().FontSize(8);
								col.Item().Text($"R$ {totalSaida:F2}").FontSize(9).Bold().FontColor("#10b981");
							});
						});

						column.Item().PaddingVertical(10).Table(table =>
						{
							table.ColumnsDefinition(columns =>
							{
								columns.RelativeColumn(2f);
								columns.RelativeColumn(1f);
								columns.RelativeColumn(1.2f);
								columns.RelativeColumn(1.2f);
							});

							table.Header(header =>
							{
								header.Cell().Background("#10b981").Padding(3).Text("Produto").Bold().FontSize(7).FontColor("#FFFFFF");
								header.Cell().Background("#10b981").Padding(3).Text("Quantidade").Bold().FontSize(7).FontColor("#FFFFFF").AlignCenter();
								header.Cell().Background("#10b981").Padding(3).Text("Preço Médio").Bold().FontSize(7).FontColor("#FFFFFF").AlignRight();
								header.Cell().Background("#10b981").Padding(3).Text("Total Saída").Bold().FontSize(7).FontColor("#FFFFFF").AlignRight();
							});

							foreach (var mov in movimentacoesPorProduto)
							{
								table.Cell().Padding(3).Text(mov.Produto?.Nome ?? "---").FontSize(7);
								table.Cell().Padding(3).Text(mov.QuantidadeSaida.ToString()).FontSize(7).AlignCenter();
								table.Cell().Padding(3).Text($"R$ {mov.PrecoMedio:F2}").FontSize(7).AlignRight();
								table.Cell().Padding(3).Text($"R$ {mov.TotalSaida:F2}").FontSize(7).AlignRight();
							}
						});
					});

					page.Footer().Element(ComposeFooter);
				});
			});

			return document.GeneratePdf();
		}

		public byte[] GerarRelatorioProdutosSemSaida(List<Produto> todosProdutos, List<Pedido> pedidosComSaida, DateTime dataInicio, DateTime dataFim)
		{
			// Produtos que não tiveram saída no período
			var idsComSaida = pedidosComSaida
				.SelectMany(p => p.Itens ?? new List<ItemPedido>())
				.Select(i => i.ProdutoId)
				.Distinct()
				.ToList();

			var produtosSemSaida = todosProdutos
				.Where(p => !idsComSaida.Contains(p.Id))
				.OrderBy(p => p.Nome)
				.ToList();

			var document = Document.Create(container =>
			{
				container.Page(page =>
				{
					page.Margin(20);
					page.DefaultTextStyle(x => x.FontSize(8));
					page.Header().Element(ComposeHeader);

					page.Content().Column(column =>
					{
						column.Item().PaddingVertical(10).Column(col =>
						{
							col.Item().Text("RELATÓRIO DE PRODUTOS SEM SAÍDA")
								  .FontSize(14).Bold().AlignCenter();
							col.Item().Text($"Período: {dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}")
								  .FontSize(8).AlignCenter().FontColor("#666");
						});

						column.Item().Row(row =>
						{
							row.RelativeItem().Column(col =>
							{
								col.Item().Text("Produtos sem Saída: ").Bold().FontSize(8);
								col.Item().Text(produtosSemSaida.Count.ToString()).FontSize(9).Bold().FontColor("#ef4444");
							});

							row.RelativeItem().Column(col =>
							{
								var percentual = (decimal)produtosSemSaida.Count / todosProdutos.Count * 100;
								col.Item().Text("% do Total: ").Bold().FontSize(8);
								col.Item().Text($"{percentual:F2}%").FontSize(9).Bold().FontColor("#ef4444");
							});
						});

						column.Item().PaddingVertical(10).Table(table =>
						{
							table.ColumnsDefinition(columns =>
							{
								columns.RelativeColumn(2f);
								columns.RelativeColumn(1.5f);
							});

							table.Header(header =>
							{
								header.Cell().Background("#10b981").Padding(3).Text("Produto").Bold().FontSize(7).FontColor("#FFFFFF");
								header.Cell().Background("#10b981").Padding(3).Text("Descrição").Bold().FontSize(7).FontColor("#FFFFFF");
							});

							foreach (var prod in produtosSemSaida)
							{
								table.Cell().Padding(3).Text(prod.Nome).FontSize(7);
								table.Cell().Padding(3).Text(prod.DescricaoCurta ?? "---").FontSize(7);
							}
						});
					});

					page.Footer().Element(ComposeFooter);
				});
			});

			return document.GeneratePdf();
		}

		public byte[] GerarRelatorioEstoqueExtrato(List<MovimentacaoEstoque> movimentacoes, List<Produto> produtos, DateTime dataInicio, DateTime dataFim)
		{
			// Agrupar movimentações por produto + tamanho
			var movimentacoesPorTamanho = movimentacoes
				.GroupBy(m => m.ProdutoTamanho)
				.Select(g => new
				{
					ProdutoTamanho = g.Key,
					Produto = g.Key?.Produto,
					Tamanho = g.Key?.Tamanho,
					Movimentacoes = g.OrderBy(m => m.DataMovimentacao).ToList(),
					TotalEntradas = g.Where(m => m.Tipo == TipoMovimentacao.Entrada || m.Tipo == TipoMovimentacao.Devolucao).Sum(m => m.Quantidade),
					TotalSaidas = g.Where(m => m.Tipo == TipoMovimentacao.Saida).Sum(m => m.Quantidade),
					TotalAjustes = g.Where(m => m.Tipo == TipoMovimentacao.Ajuste).Sum(m => m.Quantidade),
					SaldoFinal = g.OrderByDescending(m => m.DataMovimentacao).FirstOrDefault()?.EstoqueAtual ?? 0
				})
				.OrderBy(x => x.Produto?.Nome)
				.ThenBy(x => x.Tamanho)
				.ToList();

			var document = Document.Create(container =>
			{
				container.Page(page =>
				{
					page.Margin(20);
					page.DefaultTextStyle(x => x.FontSize(8));
					page.Header().Element(ComposeHeader);

					page.Content().Column(column =>
					{
						column.Item().PaddingVertical(10).Column(col =>
						{
							col.Item().Text("EXTRATO DE MOVIMENTAÇÃO DE ESTOQUE")
								  .FontSize(14).Bold().AlignCenter();
							col.Item().Text($"Período: {dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}")
								  .FontSize(8).AlignCenter().FontColor("#666");
						});

						// Verificar se há dados
						if (movimentacoesPorTamanho.Count == 0)
						{
							column.Item().PaddingVertical(40).Text("Nenhuma movimentação de estoque encontrada no período.")
								.FontSize(10)
								.AlignCenter()
								.FontColor("#666");
						}
						else
						{
							// Por cada variação de produto, mostrar um mini extrato
							foreach (var mov in movimentacoesPorTamanho)
							{
								column.Item().PaddingVertical(8).Column(col =>
								{
									col.Item().Text($"{mov.Produto?.Nome ?? "Produto Desconhecido"} - Tamanho: {mov.Tamanho}")
										  .FontSize(9)
										  .Bold()
										  .FontColor("#1f2937");

									col.Item().Row(row =>
									{
										row.RelativeItem().Column(c =>
										{
											c.Item().Text("Saldo Inicial:").FontSize(7).Bold();
											var saldoInicial = mov.Movimentacoes.FirstOrDefault()?.EstoqueAnterior ?? 0;
											c.Item().Text(saldoInicial.ToString())
												.FontSize(8)
												.Bold()
												.FontColor("#059669");
										});

										row.RelativeItem().Column(c =>
										{
											c.Item().Text("Entradas:").FontSize(7).Bold();
											c.Item().Text($"+{mov.TotalEntradas}")
												.FontSize(8)
												.Bold()
												.FontColor("#10b981");
										});

										row.RelativeItem().Column(c =>
										{
											c.Item().Text("Saídas:").FontSize(7).Bold();
											c.Item().Text($"-{mov.TotalSaidas}")
												.FontSize(8)
												.Bold()
												.FontColor("#ef4444");
										});

										row.RelativeItem().Column(c =>
										{
											c.Item().Text("Ajustes:").FontSize(7).Bold();
											var corAjuste = mov.TotalAjustes >= 0 ? "#f59e0b" : "#ef4444";
											c.Item().Text($"{(mov.TotalAjustes >= 0 ? "+" : "")}{mov.TotalAjustes}")
												.FontSize(8)
												.Bold()
												.FontColor(corAjuste);
										});

										row.RelativeItem().Column(c =>
										{
											c.Item().Text("Saldo Final:").FontSize(7).Bold();
											c.Item().Text(mov.SaldoFinal.ToString())
												.FontSize(8)
												.Bold()
												.FontColor("#0369a1");
										});
									});

									// Tabela de movimentações detalhadas
									if (mov.Movimentacoes.Count > 0)
									{
										col.Item().PaddingVertical(3).Table(table =>
										{
											table.ColumnsDefinition(columns =>
											{
												columns.RelativeColumn(1.2f);
												columns.RelativeColumn(0.8f);
												columns.RelativeColumn(1f);
												columns.RelativeColumn(1.5f);
											});

											table.Header(header =>
											{
												header.Cell().Background("#e5e7eb").Padding(2).Text("Data").Bold().FontSize(6).FontColor("#374151");
												header.Cell().Background("#e5e7eb").Padding(2).Text("Tipo").Bold().FontSize(6).FontColor("#374151");
												header.Cell().Background("#e5e7eb").Padding(2).Text("Qtd").Bold().FontSize(6).FontColor("#374151").AlignCenter();
												header.Cell().Background("#e5e7eb").Padding(2).Text("Referência").Bold().FontSize(6).FontColor("#374151");
											});

											foreach (var mov_detalhe in mov.Movimentacoes)
											{
												table.Cell().Padding(2).Text(mov_detalhe.DataMovimentacao.ToString("dd/MM/yyyy")).FontSize(6);

												var tipoDesc = mov_detalhe.Tipo switch
												{
													TipoMovimentacao.Entrada => "ENTRADA",
													TipoMovimentacao.Saida => "SAÍDA",
													TipoMovimentacao.Ajuste => "AJUSTE",
													TipoMovimentacao.Devolucao => "DEVOLUÇÃO",
													_ => "---"
												};
												table.Cell().Padding(2).Text(tipoDesc).FontSize(6);

												var corQtd = mov_detalhe.Tipo == TipoMovimentacao.Saida ? "#ef4444" : "#10b981";
												table.Cell().Padding(2).Text(mov_detalhe.Quantidade.ToString())
													.FontSize(6)
													.AlignCenter()
													.FontColor(corQtd);

												table.Cell().Padding(2).Text(mov_detalhe.Referencia ?? mov_detalhe.Motivo ?? "---").FontSize(6);
											}
										});
									}
								});
							}
						}
					});

					page.Footer().Element(ComposeFooter);
				});
			});

			return document.GeneratePdf();
		}
	}
}
