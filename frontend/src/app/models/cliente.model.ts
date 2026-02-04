export interface Cliente {
  id: string;
  nome: string;
  email: string;
  telefone?: string;
  cpf?: string;
  dataNascimento?: Date;
  emailConfirmado: boolean;
  cep?: string;
  logradouro?: string;
  numero?: string;
  complemento?: string;
  bairro?: string;
  cidade?: string;
  estado?: string;
  quantidadePedidos: number;
}

export interface CriarCliente {
  nome: string;
  email: string;
  telefone?: string;
  cpf?: string;
  dataNascimento?: Date;
  senha: string;
  cep?: string;
  logradouro?: string;
  numero?: string;
  complemento?: string;
  bairro?: string;
  cidade?: string;
  estado?: string;
}

export interface LoginCliente {
  email: string;
  senha: string;
}
