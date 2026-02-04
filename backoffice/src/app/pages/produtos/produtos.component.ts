import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProdutoService, ProdutoDTO } from '../../services/produto.service';
import { CategoriaService, CategoriaDTO } from '../../services/categoria.service';
import { AlertService } from '../../services/alert.service';
import { PaginationComponent } from '../../components/pagination/pagination.component';

@Component({
  selector: 'app-produtos',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, PaginationComponent],
  template: `
    <div class="produtos-page">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="fw-bold mb-0"><i class="bi bi-box-seam me-2"></i>Produtos</h2>
        <a routerLink="/produtos/novo" class="btn btn-primary">
          <i class="bi bi-plus-lg me-2"></i>Novo Produto
        </a>
      </div>

      <!-- Filtros -->
      <div class="card card-dashboard mb-4">
        <div class="card-body">
          <div class="row g-3">
            <div class="col-md-4">
              <div class="input-group">
                <span class="input-group-text"><i class="bi bi-search"></i></span>
                <input 
                  type="text" 
                  class="form-control" 
                  placeholder="Pesquisar produtos..."
                  [(ngModel)]="termoPesquisa"
                  (input)="pesquisar()"
                >
              </div>
            </div>
            <div class="col-md-3">
              <select class="form-select" [(ngModel)]="filtroCategoria" (change)="filtrar()">
                <option value="">Todas as Categorias</option>
                <option *ngFor="let cat of categorias" [value]="cat.id">{{ cat.nome }}</option>
              </select>
            </div>
            <div class="col-md-3">
              <select class="form-select" [(ngModel)]="filtroStatus" (change)="filtrar()">
                <option value="">Todos os Status</option>
                <option value="ativo">Ativo</option>
                <option value="inativo">Inativo</option>
              </select>
            </div>
            <div class="col-md-2">
              <button class="btn btn-outline-secondary w-100" (click)="limparFiltros()">
                <i class="bi bi-x-lg me-2"></i>Limpar
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Tabela de Produtos -->
      <div class="card card-dashboard">
        <div class="card-body p-0">
          <div class="table-responsive">
            <table class="table table-hover mb-0">
              <thead>
                <tr>
                  <th>Imagem</th>
                  <th>Produto</th>
                  <th>Categoria</th>
                  <th>Preço</th>
                  <th>Estoque mínimo</th>
                  <th>Status</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let produto of produtosPaginados">
                  <td>
                    <img 
                      [src]="produto.imagemUrl || 'assets/produto-sem-imagem.jpg'" 
                      [alt]="produto.nome"
                      class="rounded"
                      style="width: 50px; height: 50px; object-fit: cover;"
                    >
                  </td>
                  <td>
                    <strong>{{ produto.nome }}</strong>
                  </td>
                  <td>{{ produto.nomeCategoria }}</td>
                  <td>
                    <span *ngIf="produto.precoPromocional" class="text-decoration-line-through text-muted small">
                      R$ {{ produto.preco | number:'1.2-2' }}
                    </span>
                    <br *ngIf="produto.precoPromocional">
                    <span class="fw-bold" [class.text-danger]="produto.precoPromocional">
                      R$ {{ (produto.precoPromocional || produto.preco) | number:'1.2-2' }}
                    </span>
                  </td>
                  <td>
                    <span>{{ produto.quantidadeMinimaEstoque }}</span>
                  </td>
                  <td>
                    <span class="badge" [class]="produto.ativo ? 'bg-success' : 'bg-secondary'">
                      {{ produto.ativo ? 'Ativo' : 'Inativo' }}
                    </span>
                    <span *ngIf="produto.destaque" class="badge bg-warning text-dark ms-1">
                      <i class="bi bi-star-fill"></i>
                    </span>
                  </td>
                  <td>
                    <div class="btn-group">
                      <a [routerLink]="['/produtos/editar', produto.id]" class="btn btn-sm btn-outline-primary" title="Editar">
                        <i class="bi bi-pencil"></i>
                      </a>
                      <button class="btn btn-sm btn-outline-danger" title="Excluir" (click)="excluirProduto(produto)">
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
            [totalItens]="produtosFiltrados.length"
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
export class ProdutosComponent implements OnInit {
  private produtoService = inject(ProdutoService);
  private categoriaService = inject(CategoriaService);
  private alertService = inject(AlertService);

  produtos: ProdutoDTO[] = [];
  categorias: CategoriaDTO[] = [];
  produtosFiltrados: ProdutoDTO[] = [];
  produtosPaginados: ProdutoDTO[] = [];
  termoPesquisa = '';
  filtroCategoria = '';
  filtroStatus = '';
  paginaAtual = 1;
  itensPorPagina = 10;

  ngOnInit(): void {
    this.carregarCategorias();
    this.carregarProdutos();
  }

  carregarProdutos(): void {
    this.produtoService.getAll().subscribe({
      next: (list) => {
        this.produtos = list;
        this.produtosFiltrados = list;
        this.atualizarPaginacao();
      },
      error: (err) => {
        console.error('Erro carregando produtos', err);
        this.produtos = [];
        this.produtosFiltrados = [];
      }
    });
  }

  carregarCategorias(): void {
    this.categoriaService.getAll().subscribe({
      next: (list) => this.categorias = list,
      error: (err) => {
        console.error('Erro carregando categorias', err);
        this.categorias = [];
      }
    });
  }

  pesquisar(): void {
    this.filtrar();
  }

  filtrar(): void {
    this.produtosFiltrados = this.produtos.filter(produto => {
      const matchPesquisa = !this.termoPesquisa || 
        produto.nome.toLowerCase().includes(this.termoPesquisa.toLowerCase());
      
      const matchCategoria = !this.filtroCategoria || produto.categoriaId === this.filtroCategoria;
      
      const matchStatus = !this.filtroStatus || 
        (this.filtroStatus === 'ativo' && produto.ativo) ||
        (this.filtroStatus === 'inativo' && !produto.ativo);
      
      return matchPesquisa && matchCategoria && matchStatus;
    });
    
    this.paginaAtual = 1;
    this.atualizarPaginacao();
  }

  limparFiltros(): void {
    this.termoPesquisa = '';
    this.filtroCategoria = '';
    this.filtroStatus = '';
    this.produtosFiltrados = [...this.produtos];
    this.paginaAtual = 1;
    this.atualizarPaginacao();
  }

  onPaginar(event: { pagina: number; itensPorPagina: number }): void {
    this.paginaAtual = event.pagina;
    this.itensPorPagina = event.itensPorPagina;
    this.atualizarPaginacao();
  }

  private atualizarPaginacao(): void {
    const inicio = (this.paginaAtual - 1) * this.itensPorPagina;
    const fim = inicio + this.itensPorPagina;
    this.produtosPaginados = this.produtosFiltrados.slice(inicio, fim);
  }

  excluirProduto(produto: any): void {
    this.alertService.confirm('Confirmar Exclusão', `Deseja realmente excluir o produto "${produto.nome}"?`).then(confirmado => {
      if (confirmado) {
        this.produtoService.delete(produto.id).subscribe({
          next: () => {
            this.alertService.success('Produto excluído com sucesso!');
            this.carregarProdutos();
          },
          error: (err) => {
            console.error('Erro removendo produto', err);
            this.alertService.error('Erro ao remover produto', 'Tente novamente.');
          }
        });
      }
    });
  }}