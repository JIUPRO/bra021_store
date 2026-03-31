import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ClienteService, ClienteDTO } from '../../services/cliente.service';
import { AlertService } from '../../services/alert.service';
import { PaginationComponent } from '../../components/pagination/pagination.component';

@Component({
  selector: 'app-clientes',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent],
  template: `
    <div class="clientes-page">
      <div class="mb-4">
        <h2 class="fw-bold mb-0"><i class="bi bi-people me-2"></i>Clientes</h2>
      </div>

      <div class="card card-dashboard mb-4">
        <div class="card-body py-3">
          <div class="input-group filtro-nome">
            <span class="input-group-text"><i class="bi bi-search"></i></span>
            <input
              type="text"
              class="form-control"
              [(ngModel)]="filtroNome"
              (ngModelChange)="aplicarFiltro()"
              placeholder="Buscar cliente por nome">
          </div>
        </div>
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
                <tr *ngIf="!clientesPaginados.length">
                  <td colspan="6" class="text-center text-muted py-4">Nenhum cliente encontrado.</td>
                </tr>
                <tr *ngFor="let cliente of clientesPaginados">
                  <td><strong>{{ formatarNome(cliente.nome) }}</strong></td>
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

    .filtro-nome {
      min-width: 280px;
    }
  `]
})
export class ClientesComponent implements OnInit {
  private clienteService = inject(ClienteService);
  private alertService = inject(AlertService);
  private router = inject(Router);

  clientes: ClienteDTO[] = [];
  clientesFiltrados: ClienteDTO[] = [];
  clientesPaginados: ClienteDTO[] = [];
  paginaAtual = 1;
  itensPorPagina = 10;
  filtroNome = '';

  ngOnInit(): void {
    this.carregarClientes();
  }

  carregarClientes(): void {
    this.clienteService.getAll().subscribe({
      next: (list) => {
        this.clientes = list;
        this.aplicarFiltro();
      },
      error: (err) => {
        console.error('Erro carregando clientes', err);
        this.clientes = [];
      }
    });
  }

  verDetalhes(cliente: any): void {
    this.router.navigate(['/clientes', cliente.id]);
  }

  onPaginar(event: { pagina: number; itensPorPagina: number }): void {
    this.paginaAtual = event.pagina;
    this.itensPorPagina = event.itensPorPagina;
    this.atualizarPaginacao();
  }

  private atualizarPaginacao(): void {
    const inicio = (this.paginaAtual - 1) * this.itensPorPagina;
    const fim = inicio + this.itensPorPagina;
    this.clientesPaginados = this.clientesFiltrados.slice(inicio, fim);
  }

  aplicarFiltro(): void {
    const termo = this.filtroNome.trim().toLowerCase();
    this.clientesFiltrados = !termo
      ? [...this.clientes]
      : this.clientes.filter(cliente => cliente.nome.toLowerCase().includes(termo));
    this.paginaAtual = 1;
    this.atualizarPaginacao();
  }

  formatarNome(nome?: string): string {
    if (!nome) {
      return '';
    }

    return nome
      .toLowerCase()
      .split(' ')
      .filter(Boolean)
      .map(parte => parte.charAt(0).toUpperCase() + parte.slice(1))
      .join(' ');
  }
}
