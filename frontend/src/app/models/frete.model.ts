export interface CotacaoFreteItem {
  produtoId: string;
  produtoTamanhoId?: string;
  quantidade: number;
}

export interface CotacaoFreteRequest {
  cepDestino: string;
  codigoServico?: string;
  itens: CotacaoFreteItem[];
}

export interface OpcaoFrete {
  provider: string;
  codigoServico?: string;
  nomeServico: string;
  nomeTransportadora?: string;
  valor: number;
  prazoPreparacaoDias: number;
  prazoEnvioDias: number;
  prazoEntregaDias: number;
}

export interface CotacaoFreteResponse {
  freteHabilitado: boolean;
  providerConfigurado: string;
  providerUtilizado: string;
  cepOrigem: string;
  cepDestino: string;
  prazoPreparacaoDias: number;
  usandoFallbackFixo: boolean;
  mensagem?: string;
  opcoes: OpcaoFrete[];
}
