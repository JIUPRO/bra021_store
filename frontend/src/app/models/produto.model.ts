export interface Produto {
  id: string;
  nome: string;
  descricao?: string;
  descricaoCurta?: string;
  preco: number;
  precoPromocional?: number;
  valorFrete: number;
  prazoEntregaDias: number;
  imagemUrl?: string;
  quantidadeEstoque: number;
  quantidadeMinimaEstoque: number;
  destaque: boolean;
  peso: number;
  altura?: number;
  largura?: number;
  profundidade?: number;
  ativo: boolean;
  categoriaId: string;
  nomeCategoria: string;
}

export interface Categoria {
  id: string;
  nome: string;
  descricao?: string;
  imagemUrl?: string;
  ordemExibicao: number;
  quantidadeProdutos: number;
}

export interface ItemCarrinho {
  produto: Produto;
  quantidade: number;
  observacoes?: string;
  produtoVariacaoId?: string;
  tamanhoVariacao?: string;
}

export interface ProdutoVariacao {
  id: string;
  produtoId: string;
  tamanho: string;
  quantidadeEstoque: number;
  ativo: boolean;
}
