import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Pedido, ResumoPedido, StatusPedido } from '../models/pedido.model';
import { UploadService } from './upload.service';

@Injectable({ providedIn: 'root' })
export class PedidoService {
  private http = inject(HttpClient);
  private uploadService = inject(UploadService);
  private baseUrl = `${environment.apiUrl}/pedidos`;
  private pagamentoUrl = `${environment.apiUrl}/pagamentos`;
  private logisticaUrl = `${environment.apiUrl}/logistica`;

  getAll(): Observable<ResumoPedido[]> {
    return this.http.get<ResumoPedido[]>(this.baseUrl);
  }

  getById(id: string): Observable<Pedido> {
    return this.http.get<Pedido>(`${this.baseUrl}/${id}`);
  }

  getByCliente(clienteId: string): Observable<ResumoPedido[]> {
    return this.http.get<ResumoPedido[]>(`${this.baseUrl}/cliente/${clienteId}`);
  }

  create(dto: Partial<Pedido>) {
    return this.http.post<Pedido>(this.baseUrl, dto);
  }

  update(id: string, dto: Partial<Pedido>) {
    return this.http.put<Pedido>(`${this.baseUrl}/${id}`, dto);
  }

  updateStatus(id: string, novoStatus: StatusPedido) {
    return this.http.put<Pedido>(`${this.baseUrl}/${id}/status`, novoStatus);
  }

  atualizarNotaFiscal(id: string, notaFiscalUrl: string) {
    return this.http.put<Pedido>(`${this.baseUrl}/${id}/nota-fiscal`, {
      id,
      notaFiscalUrl
    });
  }

  uploadNotaFiscal(id: string, file: File) {
    return this.uploadService.uploadFile(`pedidos/${id}/nota-fiscal/upload`, file);
  }

  removerNotaFiscal(id: string) {
    return this.http.delete<Pedido>(`${this.baseUrl}/${id}/nota-fiscal`);
  }

  gerarEtiqueta(id: string) {
    return this.http.post<any>(`${this.logisticaUrl}/pedidos/${id}/gerar-etiqueta`, {});
  }

  sincronizarRastreio(id: string) {
    return this.http.post<any>(`${this.logisticaUrl}/pedidos/${id}/sincronizar-rastreio`, {});
  }

  baixarArquivoEtiqueta(id: string) {
    return this.http.get(`${this.logisticaUrl}/pedidos/${id}/arquivo-etiqueta`, {
      responseType: 'blob'
    });
  }

  cancelarPagamento(pedidoId: string): Observable<any> {
    return this.http.post(`${this.pagamentoUrl}/cancelar/${pedidoId}`, {});
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
