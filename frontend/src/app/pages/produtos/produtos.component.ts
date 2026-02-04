import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProdutoCardComponent } from '../../components/produto-card/produto-card.component';
import { ProdutoService } from '../../services/produto.service';
import { Produto, Categoria } from '../../models/produto.model';

@Component({
  selector: 'app-produtos',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, ProdutoCardComponent],
  template: `
    <div class="container py-5">
      <!-- Header com botão voltar -->
      <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 class="fw-bold">
            <i class="bi bi-grid me-2"></i>
            {{ categoriaAtual ? categoriaAtual.nome : 'Todos os Produtos' }}
          </h1>
          <p class="text-muted mb-0">
            {{ produtosFiltrados.length }} produtos encontrados
          </p>
        </div>
        <button (click)="voltar()" class="btn btn-outline-secondary">
          <i class="bi bi-arrow-left me-2"></i>Voltar
        </button>
      </div>

      <div class="row mb-4">
        <div class="col-md-12">
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
      </div>

      <div class="row">
        <!-- Sidebar - Filtros -->
        <div class="col-lg-3 mb-4">
          <div class="card">
            <div class="card-header bg-white">
              <h5 class="mb-0"><i class="bi bi-funnel me-2"></i>Filtros</h5>
            </div>
            <div class="card-body">
              <h6 class="fw-bold mb-3">Categorias</h6>
              <div class="list-group list-group-flush">
                <a 
                  routerLink="/produtos" 
                  class="list-group-item list-group-item-action"
                  [class.active]="!categoriaAtual"
                >
                  Todas as Categorias
                </a>
                <a 
                  *ngFor="let categoria of categorias"
                  [routerLink]="['/categoria', categoria.id]"
                  class="list-group-item list-group-item-action"
                  [class.active]="categoriaAtual?.id === categoria.id"
                >
                  {{ categoria.nome }}
                  <span class="badge bg-secondary float-end">{{ categoria.quantidadeProdutos }}</span>
                </a>
              </div>

              <hr>

              <h6 class="fw-bold mb-3">Ordenar por</h6>
              <select class="form-select" [(ngModel)]="ordenacao" (change)="ordenar()">
                <option value="nome">Nome (A-Z)</option>
                <option value="preco-menor">Menor Preço</option>
                <option value="preco-maior">Maior Preço</option>
                <option value="destaque">Destaques</option>
              </select>
            </div>
          </div>
        </div>

        <!-- Lista de Produtos -->
        <div class="col-lg-9">
          <div *ngIf="carregando" class="text-center py-5">
            <div class="spinner-loja mx-auto"></div>
            <p class="mt-3 text-muted">Carregando produtos...</p>
          </div>

          <div *ngIf="!carregando && produtosFiltrados.length === 0" class="text-center py-5">
            <i class="bi bi-inbox fs-1 text-muted"></i>
            <h4 class="mt-3">Nenhum produto encontrado</h4>
            <p class="text-muted">Tente ajustar seus filtros ou pesquisar por outro termo.</p>
            <button class="btn btn-primary" (click)="limparFiltros()">
              <i class="bi bi-x-circle me-2"></i>Limpar Filtros
            </button>
          </div>

          <div class="row g-4">
            <div class="col-6 col-md-4" *ngFor="let produto of produtosFiltrados">
              <app-produto-card [produto]="produto"></app-produto-card>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .spinner-loja {
      width: 50px;
      height: 50px;
      border: 4px solid #f3f3f3;
      border-top: 4px solid var(--cor-primaria);
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }
    
    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }
    
    .list-group-item.active {
      background-color: var(--cor-primaria);
      border-color: var(--cor-primaria);
      color: var(--cor-clara);
    }
  `]
})
export class ProdutosComponent implements OnInit {
  produtos: Produto[] = [];
  produtosFiltrados: Produto[] = [];
  categorias: Categoria[] = [];
  categoriaAtual: Categoria | null = null;
  termoPesquisa = '';
  ordenacao = 'nome';
  carregando = true;
  private router = inject(Router);

  constructor(
    private produtoService: ProdutoService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.carregarCategorias();
    
    this.route.params.subscribe(params => {
      const categoriaId = params['id'];
      if (categoriaId) {
        this.carregarProdutosPorCategoria(categoriaId);
        this.carregarCategoriaAtual(categoriaId);
      } else {
        this.carregarTodosProdutos();
        this.categoriaAtual = null;
      }
    });
  }

  carregarTodosProdutos(): void {
    this.carregando = true;
    this.produtoService.obterTodos().subscribe({
      next: (produtos) => {
        this.produtos = produtos;
        this.produtosFiltrados = [...produtos];
        this.carregando = false;
        this.ordenar();
      },
      error: () => {
        this.carregando = false;
      }
    });
  }

  carregarProdutosPorCategoria(categoriaId: string): void {
    this.carregando = true;
    this.produtoService.obterPorCategoria(categoriaId).subscribe({
      next: (produtos) => {
        this.produtos = produtos;
        this.produtosFiltrados = [...produtos];
        this.carregando = false;
        this.ordenar();
      },
      error: () => {
        this.carregando = false;
      }
    });
  }

  carregarCategorias(): void {
    this.produtoService.obterCategorias().subscribe({
      next: (categorias) => {
        this.categorias = categorias;
      }
    });
  }

  carregarCategoriaAtual(categoriaId: string): void {
    this.produtoService.obterCategoriaPorId(categoriaId).subscribe({
      next: (categoria) => {
        this.categoriaAtual = categoria;
      }
    });
  }

  pesquisar(): void {
    if (!this.termoPesquisa.trim()) {
      this.produtosFiltrados = [...this.produtos];
    } else {
      const termo = this.termoPesquisa.toLowerCase();
      this.produtosFiltrados = this.produtos.filter(p =>
        p.nome.toLowerCase().includes(termo) ||
        (p.descricaoCurta && p.descricaoCurta.toLowerCase().includes(termo)) ||
        p.nomeCategoria.toLowerCase().includes(termo)
      );
    }
    this.ordenar();
  }

  ordenar(): void {
    switch (this.ordenacao) {
      case 'nome':
        this.produtosFiltrados.sort((a, b) => a.nome.localeCompare(b.nome));
        break;
      case 'preco-menor':
        this.produtosFiltrados.sort((a, b) => (a.precoPromocional || a.preco) - (b.precoPromocional || b.preco));
        break;
      case 'preco-maior':
        this.produtosFiltrados.sort((a, b) => (b.precoPromocional || b.preco) - (a.precoPromocional || a.preco));
        break;
      case 'destaque':
        this.produtosFiltrados.sort((a, b) => (b.destaque ? 1 : 0) - (a.destaque ? 1 : 0));
        break;
    }
  }

  limparFiltros(): void {
    this.termoPesquisa = '';
    this.ordenacao = 'nome';
    this.produtosFiltrados = [...this.produtos];
    this.ordenar();
  }

  voltar(): void {
    this.router.navigate(['/']);
  }
}
