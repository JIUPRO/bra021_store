import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface DashboardEstatisticas {
  totalPedidos: number;
  vendasMes: number;
  totalProdutos: number;
  totalClientes: number;
}

export interface DashboardPedidoRecente {
  id: string;
  numeroPedido: string;
  nomeCliente: string;
  dataPedido: string;
  status: number;
  statusDescricao: string;
  valorTotal: number;
}

export interface DashboardAlerta {
  titulo: string;
  mensagem: string;
}

export interface DashboardEstoqueBaixo {
  produto: string;
  quantidade: number;
}

export interface DashboardDTO {
  estatisticas: DashboardEstatisticas;
  pedidosRecentes: DashboardPedidoRecente[];
  alertas: DashboardAlerta[];
  estoqueBaixo: DashboardEstoqueBaixo[];
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/dashboard`;

  getDashboard(): Observable<DashboardDTO> {
    const token = sessionStorage.getItem('token');
    const headers = token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : undefined;
    return this.http.get<DashboardDTO>(this.baseUrl, { headers });
  }
}