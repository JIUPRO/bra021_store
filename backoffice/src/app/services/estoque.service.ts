import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface MovimentacaoEstoqueDTO {
  id: string;
  quantidade: number;
  tipo: number;
  tipoDescricao: string;
  motivo?: string;
  dataMovimentacao: string;
  estoqueAnterior: number;
  estoqueAtual: number;
  referencia?: string;
  produtoTamanhoId: string;
  nomeProduto: string;
  tamanho: string;
}

export interface AlertaEstoqueDTO {
  produtoId: string;
  nomeProduto: string;
  quantidadeEstoque: number;
  quantidadeMinima: number;
  diferenca: number;
}

export interface ResumoMovimentacaoEstoqueDTO {
  produtoId?: string | null;
  produtoTamanhoId?: string | null;
  dataInicio?: string | null;
  dataFim?: string | null;
  saldoInicialPeriodo: number;
  saldoAtual: number;
  totalMovimentacoes: number;
  movimentacoes: MovimentacaoEstoqueDTO[];
}

@Injectable({ providedIn: 'root' })
export class EstoqueService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/estoque`;

  getMovimentacoes(): Observable<MovimentacaoEstoqueDTO[]> {
    return this.http.get<MovimentacaoEstoqueDTO[]>(`${this.baseUrl}/movimentacoes`);
  }

  consultarMovimentacoes(params: {
    produtoId?: string;
    produtoTamanhoId?: string;
    dataInicio?: string;
    dataFim?: string;
  }): Observable<ResumoMovimentacaoEstoqueDTO> {
    const query = new URLSearchParams();

    if (params.produtoId) {
      query.set('produtoId', params.produtoId);
    }

    if (params.produtoTamanhoId) {
      query.set('produtoTamanhoId', params.produtoTamanhoId);
    }

    if (params.dataInicio) {
      query.set('dataInicio', params.dataInicio);
    }

    if (params.dataFim) {
      query.set('dataFim', params.dataFim);
    }

    const suffix = query.toString() ? `?${query.toString()}` : '';
    return this.http.get<ResumoMovimentacaoEstoqueDTO>(`${this.baseUrl}/movimentacoes/resumo${suffix}`);
  }

  getAlertas(): Observable<AlertaEstoqueDTO[]> {
    return this.http.get<AlertaEstoqueDTO[]>(`${this.baseUrl}/alertas`);
  }

  criarMovimentacao(dto: Partial<MovimentacaoEstoqueDTO>) {
    return this.http.post<MovimentacaoEstoqueDTO>(`${this.baseUrl}/movimentacoes`, dto);
  }

  ajustarEstoque(produtoId: string, novaQuantidade: number, motivo?: string) {
    return this.http.post<void>(`${this.baseUrl}/ajustar`, {
      produtoId,
      novaQuantidade,
      motivo
    });
  }
}
