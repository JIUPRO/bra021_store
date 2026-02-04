import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Produto } from '../../models/produto.model';

@Component({
  selector: 'app-produto-card',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="card card-produto h-100">
      <div class="position-relative overflow-hidden">
        <img 
          [src]="produto.imagemUrl || 'assets/images/produto-sem-imagem.jpg'" 
          [alt]="produto.nome"
          class="card-img-top imagem-produto"
        >
        <span *ngIf="produto.destaque" class="badge bg-warning position-absolute top-0 start-0 m-2">
          <i class="bi bi-star-fill me-1"></i>Destaque
        </span>
        <span *ngIf="temDesconto" class="badge bg-danger position-absolute top-0 end-0 m-2">
          -{{ percentualDesconto }}%
        </span>
      </div>
      
      <div class="card-body d-flex flex-column">
        <small class="text-muted mb-1">{{ produto.nomeCategoria }}</small>
        <h5 class="card-title fw-bold">{{ produto.nome }}</h5>
        <p class="card-text text-muted small flex-grow-1">{{ produto.descricaoCurta }}</p>
        
        <div class="mt-auto">
          <div class="mb-2">
            <span *ngIf="temDesconto" class="preco-original me-2">
              R$ {{ produto.preco | number:'1.2-2' }}
            </span>
            <span [class]="temDesconto ? 'preco-promocional' : 'preco-normal'">
              R$ {{ precoAtual | number:'1.2-2' }}
            </span>
          </div>

          <div class="mb-2">
            <small class="text-muted">Prazo de entrega: {{ produto.prazoEntregaDias }} dias</small>
          </div>
          
          <div class="d-grid gap-2">
            <a [routerLink]="['/produto', produto.id]" class="btn btn-primario">
              <i class="bi bi-cart-check me-2"></i>Comprar
            </a>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .card-produto {
      transition: transform 0.3s ease, box-shadow 0.3s ease;
      border: none;
      border-radius: 12px;
      overflow: hidden;
      background: white;
    }
    
    .card-produto:hover {
      transform: translateY(-5px);
      box-shadow: 0 10px 30px rgba(0, 0, 0, 0.15);
    }
    
    .imagem-produto {
      height: 200px;
      object-fit: contain;
      background: #f8f9fa;
      transition: transform 0.3s ease;
    }
    
    .card-produto:hover .imagem-produto {
      transform: scale(1.05);
    }
    
    .preco-original {
      text-decoration: line-through;
      color: #6c757d;
      font-size: 0.9rem;
    }
    
    .preco-promocional {
      color: #dc3545;
      font-weight: bold;
      font-size: 1.2rem;
    }
    
    .preco-normal {
      color: var(--cor-primaria);
      font-weight: bold;
      font-size: 1.2rem;
    }
    
    .btn-primario {
      background: linear-gradient(135deg, var(--cor-primaria), var(--cor-primaria-claro));
      border: none;
      border-radius: 8px;
      padding: 10px 20px;
      font-weight: 600;
      transition: all 0.3s ease;
      color: var(--cor-escura);
    }
    
    .btn-primario:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 5px 15px rgba(47,106,73,0.18);
    }
    
    .btn-primario:disabled {
      background: var(--cor-secundaria);
      cursor: not-allowed;
    }
  `]
})
export class ProdutoCardComponent {
  @Input() produto!: Produto;

  get temDesconto(): boolean {
    return !!this.produto.precoPromocional && this.produto.precoPromocional < this.produto.preco;
  }

  get precoAtual(): number {
    return this.produto.precoPromocional || this.produto.preco;
  }

  get percentualDesconto(): number {
    if (!this.temDesconto) return 0;
    return Math.round(((this.produto.preco - this.produto.precoPromocional!) / this.produto.preco) * 100);
  }
}
