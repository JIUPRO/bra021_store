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

@Injectable({ providedIn: 'root' })
export class EstoqueService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/estoque`;

  getMovimentacoes(): Observable<MovimentacaoEstoqueDTO[]> {
    return this.http.get<MovimentacaoEstoqueDTO[]>(`${this.baseUrl}/movimentacoes`);
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
