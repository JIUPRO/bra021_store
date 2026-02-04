import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Pedido, CriarPedido, ResumoPedido } from '../models/pedido.model';

@Injectable({
  providedIn: 'root'
})
export class PedidoService {
  constructor(private api: ApiService) {}

  criar(pedido: CriarPedido): Observable<Pedido> {
    return this.api.post<Pedido>('pedidos', pedido);
  }

  obterPorId(id: string): Observable<Pedido> {
    return this.api.getById<Pedido>('pedidos', id);
  }

  obterPorCliente(clienteId: string): Observable<ResumoPedido[]> {
    return this.api.get<ResumoPedido[]>(`pedidos/cliente/${clienteId}`);
  }

  obterMeusPedidos(): Observable<ResumoPedido[]> {
    const clienteId = localStorage.getItem('clienteId');
    if (!clienteId) {
      throw new Error('Cliente não logado');
    }
    return this.obterPorCliente(clienteId);
  }
}
