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
}
