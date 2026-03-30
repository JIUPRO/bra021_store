using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Enums;
using Mapster;

namespace LojaVirtual.Aplicacao.Mapeamentos
{
	public static class MapsterConfig
	{
		private static bool _configured;

		public static void RegisterMappings()
		{
			if (_configured)
			{
				return;
			}

			_configured = true;

			TypeAdapterConfig<Categoria, CategoriaDTO>.NewConfig()
				.Map(dest => dest.QuantidadeProdutos, src => src.Produtos.Count(p => p.Ativo));
			TypeAdapterConfig<CriarCategoriaDTO, Categoria>.NewConfig();
			TypeAdapterConfig<AtualizarCategoriaDTO, Categoria>.NewConfig();

			TypeAdapterConfig<Produto, ProdutoDTO>.NewConfig()
				.Map(dest => dest.NomeCategoria, src => src.Categoria != null ? src.Categoria.Nome : string.Empty);
			TypeAdapterConfig<Produto, ProdutoResumoDTO>.NewConfig()
				.Map(dest => dest.NomeCategoria, src => src.Categoria != null ? src.Categoria.Nome : string.Empty);
			TypeAdapterConfig<CriarProdutoDTO, Produto>.NewConfig();
			TypeAdapterConfig<AtualizarProdutoDTO, Produto>.NewConfig();

			TypeAdapterConfig<Cliente, ClienteDTO>.NewConfig()
				.Map(dest => dest.QuantidadePedidos, src => src.Pedidos.Count);
			TypeAdapterConfig<CriarClienteDTO, Cliente>.NewConfig();
			TypeAdapterConfig<AtualizarClienteDTO, Cliente>.NewConfig();

			TypeAdapterConfig<Pedido, PedidoDTO>.NewConfig()
				.Map(dest => dest.StatusDescricao, src => ObterDescricaoStatus(src.Status))
				.Map(dest => dest.NomeCliente, src => src.Cliente != null ? src.Cliente.Nome : string.Empty)
				.Map(dest => dest.EmailCliente, src => src.Cliente != null ? src.Cliente.Email : string.Empty)
				.Map(dest => dest.TelefoneCliente, src => src.Cliente != null ? src.Cliente.Telefone : null)
				.Map(dest => dest.NomeEscola, src => src.Escola != null ? src.Escola.Nome : null);
			TypeAdapterConfig<Pedido, ResumoPedidoDTO>.NewConfig()
				.Map(dest => dest.StatusDescricao, src => ObterDescricaoStatus(src.Status))
				.Map(dest => dest.QuantidadeItens, src => src.Itens.Sum(i => i.Quantidade))
				.Map(dest => dest.NomeCliente, src => src.Cliente != null ? src.Cliente.Nome : string.Empty)
				.Map(dest => dest.NomeEscola, src => src.Escola != null ? src.Escola.Nome : null)
				.Map(dest => dest.Produtos, src => src.Itens.Select(i => new ResumoProdutoDTO
				{
					Nome = i.Produto != null ? i.Produto.Nome : string.Empty,
					Tamanho = i.ProdutoTamanho != null ? i.ProdutoTamanho.Tamanho : null,
					Quantidade = i.Quantidade
				}).ToList());
			TypeAdapterConfig<CriarPedidoDTO, Pedido>.NewConfig();

			TypeAdapterConfig<ItemPedido, ItemPedidoDTO>.NewConfig()
				.Map(dest => dest.NomeProduto, src => src.Produto != null ? src.Produto.Nome : string.Empty)
				.Map(dest => dest.ImagemProduto, src => src.Produto != null ? src.Produto.ImagemUrl : null)
				.Map(dest => dest.TamanhoVariacao, src => src.ProdutoTamanho != null ? src.ProdutoTamanho.Tamanho : null);

			TypeAdapterConfig<MovimentacaoEstoque, MovimentacaoEstoqueDTO>.NewConfig()
				.Map(dest => dest.TipoDescricao, src => ObterDescricaoTipoMovimentacao(src.Tipo))
				.Map(dest => dest.NomeProduto, src => src.ProdutoTamanho != null && src.ProdutoTamanho.Produto != null ? src.ProdutoTamanho.Produto.Nome : string.Empty)
				.Map(dest => dest.Tamanho, src => src.ProdutoTamanho != null ? src.ProdutoTamanho.Tamanho : string.Empty);

			TypeAdapterConfig<Escola, EscolaDTO>.NewConfig();
			TypeAdapterConfig<CriarEscolaDTO, Escola>.NewConfig();
			TypeAdapterConfig<AtualizarEscolaDTO, Escola>.NewConfig();

			TypeAdapterConfig<ParametroSistema, ParametroSistemaDTO>.NewConfig();
			TypeAdapterConfig<Usuario, UsuarioDTO>.NewConfig();
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

		private static string ObterDescricaoTipoMovimentacao(TipoMovimentacao tipo)
		{
			return tipo switch
			{
				TipoMovimentacao.Entrada => "Entrada",
				TipoMovimentacao.Saida => "Saída",
				TipoMovimentacao.Ajuste => "Ajuste",
				TipoMovimentacao.Devolucao => "Devolução",
				_ => tipo.ToString()
			};
		}
	}
}
