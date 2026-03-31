export interface ParametroSistema {
  id: string;
  chave: string;
  valor: string;
  descricao?: string;
  secao?: string;
  tipo: string;
  dataCriacao: Date;
  dataAtualizacao: Date;
}

export interface AtualizarParametro {
  valor: string;
  secao?: string;
}

export interface CriarParametro {
  chave: string;
  valor: string;
  descricao?: string;
  secao?: string;
}
