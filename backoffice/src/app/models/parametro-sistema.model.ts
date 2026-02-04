export interface ParametroSistema {
  id: string;
  chave: string;
  valor: string;
  descricao?: string;
  tipo: string;
  dataCriacao: Date;
  dataAtualizacao: Date;
}

export interface AtualizarParametro {
  valor: string;
}

export interface CriarParametro {
  chave: string;
  valor: string;
  descricao?: string;
}
