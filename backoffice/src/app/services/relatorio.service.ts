import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class RelatorioService {
  private apiUrl = `${environment.apiUrl}/relatorios`;

  constructor(private http: HttpClient) { }

  /**
   * Gera e faz download do relatório de pedidos
   * @param dataInicio Data inicial (Date object)
   * @param dataFim Data final (Date object)
   * @param status Status do pedido (opcional)
   */
  gerarRelatorioPedidos(
    dataInicio: Date,
    dataFim: Date,
    status?: number
  ): Observable<Blob> {
    const dataInicioStr = this.formatarData(dataInicio);
    const dataFimStr = this.formatarData(dataFim);

    let url = `${this.apiUrl}/pedidos?dataInicio=${dataInicioStr}&dataFim=${dataFimStr}`;

    if (status !== undefined && status !== null) {
      url += `&status=${status}`;
    }

    return this.http.get(url, { responseType: 'blob' });
  }

  /**
   * Gera e faz download do relatório de vendas
   * @param dataInicio Data inicial (Date object)
   * @param dataFim Data final (Date object)
   */
  gerarRelatorioVendas(
    dataInicio: Date,
    dataFim: Date
  ): Observable<Blob> {
    const dataInicioStr = this.formatarData(dataInicio);
    const dataFimStr = this.formatarData(dataFim);

    const url = `${this.apiUrl}/vendas?dataInicio=${dataInicioStr}&dataFim=${dataFimStr}`;
    return this.http.get(url, { responseType: 'blob' });
  }

  /**
   * Gera e faz download do relatório de comissão
   * @param dataInicio Data inicial (Date object)
   * @param dataFim Data final (Date object)
   */
  gerarRelatorioComissao(
    dataInicio: Date,
    dataFim: Date
  ): Observable<Blob> {
    const dataInicioStr = this.formatarData(dataInicio);
    const dataFimStr = this.formatarData(dataFim);

    const url = `${this.apiUrl}/comissao?dataInicio=${dataInicioStr}&dataFim=${dataFimStr}`;
    return this.http.get(url, { responseType: 'blob' });
  }

  /**
   * Gera e faz download do relatório de produtos mais vendidos
   * @param dataInicio Data inicial (Date object)
   * @param dataFim Data final (Date object)
   */
  gerarRelatorioProdutosMaisVendidos(
    dataInicio: Date,
    dataFim: Date
  ): Observable<Blob> {
    const dataInicioStr = this.formatarData(dataInicio);
    const dataFimStr = this.formatarData(dataFim);

    const url = `${this.apiUrl}/produtos-vendidos?dataInicio=${dataInicioStr}&dataFim=${dataFimStr}`;
    return this.http.get(url, { responseType: 'blob' });
  }

  /**
   * Gera e faz download do relatório de clientes
   * @param dataInicio Data inicial (Date object)
   * @param dataFim Data final (Date object)
   */
  gerarRelatorioClientes(
    dataInicio: Date,
    dataFim: Date
  ): Observable<Blob> {
    const dataInicioStr = this.formatarData(dataInicio);
    const dataFimStr = this.formatarData(dataFim);

    const url = `${this.apiUrl}/clientes?dataInicio=${dataInicioStr}&dataFim=${dataFimStr}`;
    return this.http.get(url, { responseType: 'blob' });
  }

  /**
   * Gera e faz download do relatório de estoque
   * @param dataInicio Data inicial (Date object)
   * @param dataFim Data final (Date object)
   */
  gerarRelatorioEstoque(
    dataInicio: Date,
    dataFim: Date
  ): Observable<Blob> {
    const dataInicioStr = this.formatarData(dataInicio);
    const dataFimStr = this.formatarData(dataFim);

    const url = `${this.apiUrl}/estoque?dataInicio=${dataInicioStr}&dataFim=${dataFimStr}`;
    return this.http.get(url, { responseType: 'blob' });
  }

  /**
   * Gera e faz download do relatório de produtos sem saída
   * @param dataInicio Data inicial (Date object)
   * @param dataFim Data final (Date object)
   */
  gerarRelatorioProdutosSemSaida(
    dataInicio: Date,
    dataFim: Date
  ): Observable<Blob> {
    const dataInicioStr = this.formatarData(dataInicio);
    const dataFimStr = this.formatarData(dataFim);

    const url = `${this.apiUrl}/produtos-sem-saida?dataInicio=${dataInicioStr}&dataFim=${dataFimStr}`;
    return this.http.get(url, { responseType: 'blob' });
  }

  /**
   * Faz download do blob como arquivo
   * @param blob Arquivo PDF
   * @param nomeArquivo Nome do arquivo a salvar
   */
  downloadPDF(blob: Blob, nomeArquivo: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = nomeArquivo;
    link.click();
    window.URL.revokeObjectURL(url);
  }

  /**
   * Formata data para o padrão yyyy-MM-dd esperado pela API
   */
  private formatarData(data: Date): string {
    const ano = data.getFullYear();
    const mes = String(data.getMonth() + 1).padStart(2, '0');
    const dia = String(data.getDate()).padStart(2, '0');
    return `${ano}-${mes}-${dia}`;
  }
}
