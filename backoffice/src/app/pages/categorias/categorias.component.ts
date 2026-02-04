import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CategoriaService, CategoriaDTO } from '../../services/categoria.service';
import { AlertService } from '../../services/alert.service';
import { PaginationComponent } from '../../components/pagination/pagination.component';

@Component({
  selector: 'app-categorias',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, ReactiveFormsModule, PaginationComponent],
  template: `
    <div class="categorias-page">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="fw-bold mb-0"><i class="bi bi-tags me-2"></i>Categorias</h2>
        <button class="btn btn-primary" (click)="abrirFormulario()">
          <i class="bi bi-plus-lg me-2"></i>Nova Categoria
        </button>
      </div>

      <div class="card card-dashboard">
        <div class="card-body p-0">
          <div class="table-responsive">
            <table class="table table-hover mb-0">
              <thead>
                <tr>
                  <th>Nome</th>
                  <th>Descrição</th>
                  <th>Produtos</th>
                  <th>Ordem</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let categoria of categoriasPaginadas">
                  <td><strong>{{ categoria.nome }}</strong></td>
                  <td>{{ categoria.descricao }}</td>
                  <td>{{ categoria.quantidadeProdutos || 0 }}</td>
                  <td>{{ categoria.ordemExibicao }}</td>
                  <td>
                    <div class="btn-group">
                      <button class="btn btn-sm btn-outline-primary" (click)="editarCategoria(categoria)">
                        <i class="bi bi-pencil"></i>
                      </button>
                      <button class="btn btn-sm btn-outline-danger" (click)="excluirCategoria(categoria)">
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
            [totalItens]="categorias.length"
            [paginaAtual]="paginaAtual"
            [itensPorPagina]="itensPorPagina"
            (paginar)="onPaginar($event)">
          </app-pagination>
        </div>
      </div>
    </div>

    <!-- Modal para criar/editar categoria -->
    <div class="modal" [class.show]="mostrarFormulario" [style.display]="mostrarFormulario ? 'block' : 'none'">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">{{ editando ? 'Editar Categoria' : 'Nova Categoria' }}</h5>
            <button type="button" class="btn-close" (click)="fecharFormulario()"></button>
          </div>
          <form [formGroup]="formulario" (ngSubmit)="salvarCategoria()">
            <div class="modal-body">
              <div class="mb-3">
                <label for="nome" class="form-label">Nome *</label>
                <input 
                  type="text" 
                  class="form-control" 
                  id="nome" 
                  formControlName="nome"
                  placeholder="Ex: Eletrônicos">
              </div>
              <div class="mb-3">
                <label for="descricao" class="form-label">Descrição</label>
                <textarea 
                  class="form-control" 
                  id="descricao" 
                  formControlName="descricao"
                  rows="3"
                  placeholder="Descrição da categoria"></textarea>
              </div>
              <div class="mb-3">
                <label for="ordemExibicao" class="form-label">Ordem de Exibição</label>
                <input 
                  type="number" 
                  class="form-control" 
                  id="ordemExibicao" 
                  formControlName="ordemExibicao"
                  placeholder="1">
              </div>
              <div class="mb-3">
                <label for="imagemUrl" class="form-label">URL da Imagem</label>
                <input 
                  type="text" 
                  class="form-control" 
                  id="imagemUrl" 
                  formControlName="imagemUrl"
                  placeholder="https://...">
              </div>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" (click)="fecharFormulario()">Cancelar</button>
              <button type="submit" class="btn btn-primary" [disabled]="formulario.invalid || carregando">
                <span *ngIf="carregando" class="spinner-border spinner-border-sm me-2"></span>
                {{ editando ? 'Atualizar' : 'Criar' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Backdrop do modal -->
    <div class="modal-backdrop fade" [class.show]="mostrarFormulario" *ngIf="mostrarFormulario"></div>
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

    .modal.show {
      z-index: 1050;
    }

    .modal-backdrop.show {
      opacity: 0.5;
      z-index: 1040;
    }
  `]
})
export class CategoriasComponent implements OnInit {
  private categoriaService = inject(CategoriaService);
  private fb = inject(FormBuilder);
  private alertService = inject(AlertService);

  categorias: CategoriaDTO[] = [];
  categoriasPaginadas: CategoriaDTO[] = [];
  mostrarFormulario = false;
  editando = false;
  carregando = false;
  formulario: FormGroup;
  paginaAtual = 1;
  itensPorPagina = 10;

  constructor() {
    this.formulario = this.fb.group({
      id: [''],
      nome: ['', [Validators.required, Validators.minLength(3)]],
      descricao: [''],
      ordemExibicao: [0],
      imagemUrl: ['']
    });
  }

  ngOnInit(): void {
    this.carregarCategorias();
  }

  carregarCategorias(): void {
    this.categoriaService.getAll().subscribe({
      next: (list) => {
        this.categorias = list;
        this.atualizarPaginacao();
      },
      error: (err) => {
        console.error('Erro carregando categorias', err);
        this.categorias = [];
      }
    });
  }

  abrirFormulario(): void {
    this.editando = false;
    this.formulario.reset();
    this.mostrarFormulario = true;
  }

  fecharFormulario(): void {
    this.mostrarFormulario = false;
    this.formulario.reset();
  }

  onPaginar(event: { pagina: number; itensPorPagina: number }): void {
    this.paginaAtual = event.pagina;
    this.itensPorPagina = event.itensPorPagina;
    this.atualizarPaginacao();
  }

  private atualizarPaginacao(): void {
    const inicio = (this.paginaAtual - 1) * this.itensPorPagina;
    const fim = inicio + this.itensPorPagina;
    this.categoriasPaginadas = this.categorias.slice(inicio, fim);
  }

  salvarCategoria(): void {
    if (this.formulario.invalid) return;

    this.carregando = true;
    const dados = this.formulario.value;

    const requisicao = this.editando 
      ? this.categoriaService.update(this.formulario.get('id')?.value, dados)
      : this.categoriaService.create(dados);

    requisicao.subscribe({
      next: () => {
        this.carregarCategorias();
        this.fecharFormulario();
        this.carregando = false;
      },
      error: (err) => {
        console.error('Erro ao salvar categoria', err);
        this.alertService.error('Erro ao salvar categoria', (err.error?.mensagem || err.message));
        this.carregando = false;
      }
    });
  }

  editarCategoria(categoria: any): void {
    this.editando = true;
    this.formulario.patchValue({
      id: categoria.id,
      nome: categoria.nome,
      descricao: categoria.descricao,
      ordemExibicao: categoria.ordemExibicao,
      imagemUrl: categoria.imagemUrl
    });
    this.mostrarFormulario = true;
  }

  excluirCategoria(categoria: any): void {
    this.alertService.confirm('Confirmar Exclusão', `Deseja excluir a categoria "${categoria.nome}"?`).then(confirmado => {
      if (confirmado) {
        this.categoriaService.delete(categoria.id).subscribe({
          next: () => this.carregarCategorias(),
          error: (err) => {
            console.error('Erro removendo categoria', err);
            this.alertService.error('Erro ao remover', (err.error?.mensagem || err.message));
          }
        });
      }
    });
  }
}
