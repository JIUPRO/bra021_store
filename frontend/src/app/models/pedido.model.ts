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
  produtoTamanhoId?: string;
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
  prazoPreparacaoDias: number;
  prazoEnvioDias: number;
  prazoEntregaDias: number;
  observacoes?: string;
  metodoPagamento?: string;
  notaFiscalUrl?: string;
  tipoEntrega?: string;
  transportadoraFrete?: string;
  servicoFrete?: string;
  codigoServicoFrete?: string;
  providerFreteUtilizado?: string;
  providerLogisticaUtilizado?: string;
  integracaoFretePedidoId?: string;
  integracaoFreteProtocolo?: string;
  codigoRastreio?: string;
  urlRastreio?: string;
  urlEtiqueta?: string;
  statusLogistico?: string;
  dataEtiquetaGerada?: Date;
  dataPostagem?: Date;
  dataEntrega?: Date;
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
  itens: ItemPedido[];
}

export interface CriarItemPedido {
  produtoId: string;
  quantidade: number;
  observacoes?: string;
  produtoTamanhoId?: string;
}

export interface CriarPedido {
  clienteId: string;
  escolaId?: string;
  itens: CriarItemPedido[];
  valorFrete: number;
  valorDesconto: number;
  prazoPreparacaoDias: number;
  prazoEnvioDias: number;
  prazoEntregaDias: number;
  observacoes?: string;
  nomeEntrega: string;
  telefoneEntrega: string;
  cepEntrega: string;
  logradouroEntrega: string;
  numeroEntrega: string;
  complementoEntrega?: string;
  bairroEntrega: string;
  cidadeEntrega: string;
  estadoEntrega: string;
  tipoEntrega?: string;
  providerFrete?: string;
  transportadoraFrete?: string;
  codigoServicoFrete?: string;
  nomeServicoFrete?: string;
}

export interface ResumoPedido {
  id: string;
  numeroPedido: string;
  dataPedido: Date;
  status: StatusPedido;
  statusDescricao: string;
  valorTotal: number;
  quantidadeItens: number;
  prazoPreparacaoDias: number;
  prazoEnvioDias: number;
  prazoEntregaDias: number;
  nomeCliente: string;
  metodoPagamento?: string;
  tipoEntrega?: string;
  transportadoraFrete?: string;
  servicoFrete?: string;
  providerFreteUtilizado?: string;
  providerLogisticaUtilizado?: string;
  codigoRastreio?: string;
  statusLogistico?: string;
  dataPostagem?: Date;
  dataEntrega?: Date;
  produtos?: Array<{
    nome: string;
    tamanho?: string;
    quantidade: number;
  }>;
}
