import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ClienteService, ClienteDTO } from '../../services/cliente.service';
import { AlertService } from '../../services/alert.service';
import { PaginationComponent } from '../../components/pagination/pagination.component';

@Component({
  selector: 'app-clientes',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, PaginationComponent],
  template: `
    <div class="clientes-page">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="fw-bold mb-0"><i class="bi bi-people me-2"></i>Clientes</h2>
        <button class="btn btn-outline-primary" (click)="exportar()">
          <i class="bi bi-download me-2"></i>Exportar
        </button>
      </div>

      <div class="card card-dashboard">
        <div class="card-body p-0">
          <div class="table-responsive">
            <table class="table table-hover mb-0">
              <thead>
                <tr>
                  <th>Nome</th>
                  <th>Email</th>
                  <th>Telefone</th>
                  <th>Cidade</th>
                  <th>Pedidos</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let cliente of clientesPaginados">
                  <td><strong>{{ cliente.nome }}</strong></td>
                  <td>{{ cliente.email }}</td>
                  <td>{{ cliente.telefone }}</td>
                  <td>{{ cliente.cidade }}</td>
                  <td>{{ cliente.quantidadePedidos }}</td>
                  <td>
                    <button class="btn btn-sm btn-outline-primary" (click)="verDetalhes(cliente)">
                      <i class="bi bi-eye"></i>
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
        <div class="card-footer bg-white">
          <app-pagination
            [totalItens]="clientes.length"
            [paginaAtual]="paginaAtual"
            [itensPorPagina]="itensPorPagina"
            (paginar)="onPaginar($event)">
          </app-pagination>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .card-dashboard {
      border: none;
      border-radius: 12px;
      box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
    }
    
    .table th {
      font-weight: 600;
      color: #212529;
      border: none;
      background: #f8f9fa;
      padding: 16px;
    }
    
    .table td {
      vertical-align: middle;
      padding: 16px;
    }
  `]
})
export class ClientesComponent implements OnInit {
  private clienteService = inject(ClienteService);
  private alertService = inject(AlertService);

  clientes: ClienteDTO[] = [];
  clientesPaginados: ClienteDTO[] = [];
  paginaAtual = 1;
  itensPorPagina = 10;

  ngOnInit(): void {
    this.carregarClientes();
  }

  carregarClientes(): void {
    this.clienteService.getAll().subscribe({
      next: (list) => {
        this.clientes = list;
        this.atualizarPaginacao();
      },
      error: (err) => {
        console.error('Erro carregando clientes', err);
        this.clientes = [];
      }
    });
  }

  exportar(): void {
    // Implementar exportação em CSV/Excel
    this.alertService.info('Exportar', 'Exportando clientes...');
  }

  verDetalhes(cliente: any): void {
    this.alertService.info('Cliente', 'Detalhes do cliente: ' + cliente.nome);
  }

  onPaginar(event: { pagina: number; itensPorPagina: number }): void {
    this.paginaAtual = event.pagina;
    this.itensPorPagina = event.itensPorPagina;
    this.atualizarPaginacao();
  }

  private atualizarPaginacao(): void {
    const inicio = (this.paginaAtual - 1) * this.itensPorPagina;
    const fim = inicio + this.itensPorPagina;
    this.clientesPaginados = this.clientes.slice(inicio, fim);
  }
}
