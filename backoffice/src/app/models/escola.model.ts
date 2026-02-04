export interface Escola {
  id: string;
  nome: string;
  cep?: string;
  logradouro?: string;
  numero?: string;
  complemento?: string;
  bairro?: string;
  cidade?: string;
  estado?: string;
  contato?: string;
  professorResponsavel?: string;
  percentualComissao: number;
  ativo: boolean;
  dataCriacao: Date;
}

export interface CriarEscola {
  nome: string;
  cep?: string;
  logradouro?: string;
  numero?: string;
  complemento?: string;
  bairro?: string;
  cidade?: string;
  estado?: string;
  contato?: string;
  professorResponsavel?: string;
  percentualComissao: number;
}

export interface AtualizarEscola {
  id: string;
  nome: string;
  cep?: string;
  logradouro?: string;
  numero?: string;
  complemento?: string;
  bairro?: string;
  cidade?: string;
  estado?: string;
  contato?: string;
  professorResponsavel?: string;
  percentualComissao: number;
  ativo: boolean;
}
