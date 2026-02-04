using AutoMapper;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Enums;
using LojaVirtual.Aplicacao.DTOs;

namespace LojaVirtual.Aplicacao.Mapeamentos
{
	public class MapeamentoPerfil : Profile
	{
		public MapeamentoPerfil()
		{
			// Mapeamento de Categoria
			CreateMap<Categoria, CategoriaDTO>()
				 .ForMember(dest => dest.QuantidadeProdutos, opt => opt.MapFrom(src => src.Produtos.Count(p => p.Ativo)));
			CreateMap<CriarCategoriaDTO, Categoria>();
			CreateMap<AtualizarCategoriaDTO, Categoria>();

			// Mapeamento de Produto
			CreateMap<Produto, ProdutoDTO>()
				 .ForMember(dest => dest.NomeCategoria, opt => opt.MapFrom(src => src.Categoria.Nome));
			CreateMap<Produto, ProdutoResumoDTO>()
				 .ForMember(dest => dest.NomeCategoria, opt => opt.MapFrom(src => src.Categoria.Nome));
			CreateMap<CriarProdutoDTO, Produto>();
			CreateMap<AtualizarProdutoDTO, Produto>();

			// Mapeamento de Cliente
			CreateMap<Cliente, ClienteDTO>()
				 .ForMember(dest => dest.QuantidadePedidos, opt => opt.MapFrom(src => src.Pedidos.Count));
			CreateMap<CriarClienteDTO, Cliente>();
			CreateMap<AtualizarClienteDTO, Cliente>();

			// Mapeamento de Pedido
			CreateMap<Pedido, PedidoDTO>()
				 .ForMember(dest => dest.StatusDescricao, opt => opt.MapFrom(src => ObterDescricaoStatus(src.Status)))
				 .ForMember(dest => dest.NomeCliente, opt => opt.MapFrom(src => src.Cliente.Nome))
				 .ForMember(dest => dest.EmailCliente, opt => opt.MapFrom(src => src.Cliente.Email))
				 .ForMember(dest => dest.TelefoneCliente, opt => opt.MapFrom(src => src.Cliente.Telefone))
				 .ForMember(dest => dest.NomeEscola, opt => opt.MapFrom(src => src.Escola != null ? src.Escola.Nome : null));
			CreateMap<Pedido, ResumoPedidoDTO>()
				 .ForMember(dest => dest.StatusDescricao, opt => opt.MapFrom(src => ObterDescricaoStatus(src.Status)))
				 .ForMember(dest => dest.QuantidadeItens, opt => opt.MapFrom(src => src.Itens.Sum(i => i.Quantidade)))
				 .ForMember(dest => dest.NomeCliente, opt => opt.MapFrom(src => src.Cliente.Nome))
				 .ForMember(dest => dest.NomeEscola, opt => opt.MapFrom(src => src.Escola != null ? src.Escola.Nome : null))
				 .ForMember(dest => dest.Produtos, opt => opt.MapFrom(src => src.Itens.Select(i => new ResumoProdutoDTO
				 {
					 Nome = i.Produto.Nome,
					 Tamanho = i.ProdutoTamanho != null ? i.ProdutoTamanho.Tamanho : null,
					 Quantidade = i.Quantidade
				 }).ToList()));
			CreateMap<CriarPedidoDTO, Pedido>();

			// Mapeamento de ItemPedido
			CreateMap<ItemPedido, ItemPedidoDTO>()
				 .ForMember(dest => dest.NomeProduto, opt => opt.MapFrom(src => src.Produto.Nome))
				 .ForMember(dest => dest.ImagemProduto, opt => opt.MapFrom(src => src.Produto.ImagemUrl))
				 .ForMember(dest => dest.TamanhoVariacao, opt => opt.MapFrom(src => src.ProdutoTamanho != null ? src.ProdutoTamanho.Tamanho : null));

			// Mapeamento de MovimentacaoEstoque
			CreateMap<MovimentacaoEstoque, MovimentacaoEstoqueDTO>()
				 .ForMember(dest => dest.TipoDescricao, opt => opt.MapFrom(src => ObterDescricaoTipoMovimentacao(src.Tipo)))
				 .ForMember(dest => dest.NomeProduto, opt => opt.MapFrom(src => src.ProdutoTamanho.Produto.Nome))
				 .ForMember(dest => dest.Tamanho, opt => opt.MapFrom(src => src.ProdutoTamanho.Tamanho));

			// Mapeamento de Escola
			CreateMap<Escola, EscolaDTO>();
			CreateMap<CriarEscolaDTO, Escola>();
			CreateMap<AtualizarEscolaDTO, Escola>();

			// Mapeamento de ParametroSistema
			CreateMap<ParametroSistema, ParametroSistemaDTO>();

			// Mapeamento de Usuario
			CreateMap<Usuario, UsuarioDTO>();
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
