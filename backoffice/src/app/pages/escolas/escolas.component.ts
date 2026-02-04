import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { EscolaService } from '../../services/escola.service';
import { AlertService } from '../../services/alert.service';
import { Escola } from '../../models/escola.model';
import { PaginationComponent } from '../../components/pagination/pagination.component';

@Component({
  selector: 'app-escolas',
  standalone: true,
  imports: [CommonModule, RouterLink, PaginationComponent],
  template: `
    <div class="container-fluid py-4">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h1 class="fw-bold">
          <i class="bi bi-building me-2"></i>Escolas
        </h1>
        <a routerLink="/escolas/nova" class="btn btn-primary">
          <i class="bi bi-plus-lg me-2"></i>Nova Escola
        </a>
      </div>

      <div *ngIf="carregando" class="text-center py-5">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Carregando...</span>
        </div>
      </div>

      <div *ngIf="!carregando && escolas.length === 0" class="alert alert-info">
        <i class="bi bi-info-circle me-2"></i>
        Nenhuma escola cadastrada ainda.
      </div>

      <div *ngIf="!carregando && escolas.length > 0" class="card">
        <div class="card-body">
          <div class="table-responsive">
            <table class="table table-hover">
              <thead>
                <tr>
                  <th>Nome</th>
                  <th>Professor Responsável</th>
                  <th>Contato</th>
                  <th>Comissão</th>
                  <th>Status</th>
                  <th width="150">Ações</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let escola of escolasPaginadas">
                  <td>{{ escola.nome }}</td>
                  <td>{{ escola.professorResponsavel || '—' }}</td>
                  <td>{{ escola.contato || '—' }}</td>
                  <td>{{ escola.percentualComissao | number:'1.2-2' }}%</td>
                  <td>
                    <span class="badge" [class.bg-success]="escola.ativo" [class.bg-secondary]="!escola.ativo">
                      {{ escola.ativo ? 'Ativa' : 'Inativa' }}
                    </span>
                  </td>
                  <td>
                    <div class="btn-group btn-group-sm">
                      <a [routerLink]="['/escolas/editar', escola.id]" class="btn btn-outline-primary" title="Editar">
                        <i class="bi bi-pencil"></i>
                      </a>
                      <button (click)="confirmarRemocao(escola)" class="btn btn-outline-danger" title="Remover">
                        <i class="bi bi-trash"></i>
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
        <div class="card-footer bg-white">
          <app-pagination
            [totalItens]="escolas.length"
            [paginaAtual]="paginaAtual"
            [itensPorPagina]="itensPorPagina"
            (paginar)="onPaginar($event)">
          </app-pagination>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class EscolasComponent implements OnInit {
  escolas: Escola[] = [];
  escolasPaginadas: Escola[] = [];
  carregando = false;
  paginaAtual = 1;
  itensPorPagina = 10;

  constructor(
    private escolaService: EscolaService,
    private alertService: AlertService
  ) {}

  ngOnInit(): void {
    this.carregarEscolas();
  }

  carregarEscolas(): void {
    this.carregando = true;
    this.escolaService.obterTodas().subscribe({
      next: (escolas) => {
        this.escolas = escolas;
        this.atualizarPaginacao();
        this.carregando = false;
      },
      error: (err) => {
        console.error('Erro ao carregar escolas', err);
        this.alertService.error('Erro', 'Erro ao carregar escolas');
        this.carregando = false;
      }
    });
  }

  confirmarRemocao(escola: Escola): void {
    if (confirm(`Deseja realmente remover a escola "${escola.nome}"?`)) {
      this.escolaService.remover(escola.id).subscribe({
        next: () => {
          this.alertService.success('Sucesso', 'Escola removida com sucesso');
          this.carregarEscolas();
        },
        error: (err) => {
          console.error('Erro ao remover escola', err);
          this.alertService.error('Erro', 'Erro ao remover escola');
        }
      });
    }
  }

  onPaginar(event: { pagina: number; itensPorPagina: number }): void {
    this.paginaAtual = event.pagina;
    this.itensPorPagina = event.itensPorPagina;
    this.atualizarPaginacao();
  }

  private atualizarPaginacao(): void {
    const inicio = (this.paginaAtual - 1) * this.itensPorPagina;
    const fim = inicio + this.itensPorPagina;
    this.escolasPaginadas = this.escolas.slice(inicio, fim);
  }
}
