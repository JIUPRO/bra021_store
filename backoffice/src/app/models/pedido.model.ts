export enum StatusPedido {
  Pendente = 0,
  AguardandoPagamento = 1,
  Pago = 2,
  EmSeparacao = 3,
  Enviado = 4,
  Entregue = 5,
  Cancelado = 6
}

export interface ItemPedido {
  id: string;
  quantidade: number;
  precoUnitario: number;
  valorTotal: number;
  observacoes?: string;
  produtoId: string;
  nomeProduto: string;
  imagemProduto?: string;
  produtoVariacaoId?: string;
  tamanhoVariacao?: string;
}

export interface Pedido {
  id: string;
  numeroPedido: string;
  dataPedido: Date;
  status: StatusPedido;
  statusDescricao: string;
  valorSubtotal: number;
  valorFrete: number;
  valorDesconto: number;
  valorTotal: number;
  prazoEntregaDias: number;
  observacoes?: string;
  pagamentoId?: string;
  metodoPagamento?: string;
  notaFiscalUrl?: string;
  nomeEntrega: string;
  telefoneEntrega: string;
  cepEntrega: string;
  logradouroEntrega: string;
  numeroEntrega: string;
  complementoEntrega?: string;
  bairroEntrega: string;
  cidadeEntrega: string;
  estadoEntrega: string;
  clienteId: string;
  nomeCliente: string;
  emailCliente: string;
  telefoneCliente: string;
  escolaId?: string;
  nomeEscola?: string;
  itens: ItemPedido[];
}

export interface CriarItemPedido {
  produtoId: string;
  produtoVariacaoId?: string;
  quantidade: number;
  observacoes?: string;
}

export interface CriarPedido {
  clienteId: string;
  escolaId?: string;
  itens: CriarItemPedido[];
  valorFrete: number;
  valorDesconto: number;
}

export interface AtualizarStatusPedido {
  status: StatusPedido;
}

export interface ResumoPedido {
  id: string;
  numeroPedido: string;
  dataPedido: Date;
  status: StatusPedido;
  statusDescricao: string;
  nomeCliente: string;
  quantidadeItens: number;
  valorTotal: number;
  prazoEntregaDias: number;
  pagamentoId?: string;
  metodoPagamento?: string;
}
