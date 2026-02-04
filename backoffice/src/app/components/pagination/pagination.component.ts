import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="d-flex justify-content-between align-items-center gap-3 flex-wrap">
      <div class="d-flex align-items-center gap-2">
        <label for="registrosPorPagina" class="small text-muted mb-0">Registros por página:</label>
        <select 
          id="registrosPorPagina"
          class="form-select form-select-sm" 
          style="width: auto;"
          [(ngModel)]="itensPorPagina"
          (change)="onItensPorPaginaChange()">
          <option value="5">5</option>
          <option value="10">10</option>
          <option value="25">25</option>
          <option value="50">50</option>
          <option value="100">100</option>
        </select>
      </div>

      <small class="text-muted">
        Mostrando {{ (paginaAtual - 1) * itensPorPagina + 1 }} a {{ Math.min(paginaAtual * itensPorPagina, totalItens) }} de {{ totalItens }} registros
      </small>

      <nav aria-label="Paginação">
        <ul class="pagination pagination-sm mb-0">
          <li class="page-item" [class.disabled]="paginaAtual === 1">
            <button 
              class="page-link" 
              (click)="irParaPagina(1)"
              [disabled]="paginaAtual === 1">
              <i class="bi bi-chevron-double-left"></i> Primeira
            </button>
          </li>

          <li class="page-item" [class.disabled]="paginaAtual === 1">
            <button 
              class="page-link" 
              (click)="irParaPagina(paginaAtual - 1)"
              [disabled]="paginaAtual === 1">
              <i class="bi bi-chevron-left"></i> Anterior
            </button>
          </li>

          <li class="page-item" *ngFor="let pagina of paginas">
            <button 
              class="page-link"
              [class.active]="pagina === paginaAtual"
              (click)="irParaPagina(pagina)"
              *ngIf="!isEllipsis(pagina)">
              {{ pagina }}
            </button>
            <span class="page-link" *ngIf="isEllipsis(pagina)">...</span>
          </li>

          <li class="page-item" [class.disabled]="paginaAtual === totalPaginas">
            <button 
              class="page-link" 
              (click)="irParaPagina(paginaAtual + 1)"
              [disabled]="paginaAtual === totalPaginas">
              Próxima <i class="bi bi-chevron-right"></i>
            </button>
          </li>

          <li class="page-item" [class.disabled]="paginaAtual === totalPaginas">
            <button 
              class="page-link" 
              (click)="irParaPagina(totalPaginas)"
              [disabled]="paginaAtual === totalPaginas">
              Última <i class="bi bi-chevron-double-right"></i>
            </button>
          </li>
        </ul>
      </nav>
    </div>
  `,
  styles: [`
    .pagination {
      flex-wrap: wrap;
    }

    .page-link {
      cursor: pointer;
      border: 1px solid #dee2e6;
      color: #0d6efd;
      padding: 0.375rem 0.75rem;
      font-size: 0.875rem;
    }

    .page-link:hover:not(:disabled) {
      background-color: #e9ecef;
      color: #0d6efd;
    }

    .page-item.active .page-link {
      background-color: #0d6efd;
      border-color: #0d6efd;
      color: white;
    }

    .page-item.disabled .page-link {
      cursor: not-allowed;
      opacity: 0.5;
      color: #6c757d;
    }

    .form-select-sm {
      padding: 0.25rem 0.5rem;
      font-size: 0.875rem;
      padding-right: 2rem;
      background-position: right 0.6rem center;
      background-size: 12px 12px;
    }
  `]
})
export class PaginationComponent {
  @Input() totalItens: number = 0;
  @Input() paginaAtual: number = 1;
  @Input() itensPorPagina: number = 10;
  @Output() paginar = new EventEmitter<{ pagina: number; itensPorPagina: number }>();

  Math = Math;

  get totalPaginas(): number {
    return Math.ceil(this.totalItens / this.itensPorPagina);
  }

  get paginas(): (number | string)[] {
    const total = this.totalPaginas;
    const atual = this.paginaAtual;
    const paginas: (number | string)[] = [];

    // Primeira página
    paginas.push(1);

    // Ellipsis antes
    if (atual > 3) {
      paginas.push('...');
    }

    // Páginas ao redor da atual
    for (let i = Math.max(2, atual - 1); i <= Math.min(total - 1, atual + 1); i++) {
      if (!paginas.includes(i)) {
        paginas.push(i);
      }
    }

    // Ellipsis depois
    if (atual < total - 2) {
      paginas.push('...');
    }

    // Última página
    if (total > 1 && !paginas.includes(total)) {
      paginas.push(total);
    }

    return paginas;
  }

  isEllipsis(pagina: number | string): boolean {
    return pagina === '...';
  }

  irParaPagina(pagina: number | string): void {
    if (this.isEllipsis(pagina)) {
      return;
    }

    const paginaNumero = pagina as number;

    if (paginaNumero >= 1 && paginaNumero <= this.totalPaginas) {
      this.paginaAtual = paginaNumero;
      this.emitirPaginacao();
    }
  }

  onItensPorPaginaChange(): void {
    this.paginaAtual = 1;
    this.emitirPaginacao();
  }

  private emitirPaginacao(): void {
    this.paginar.emit({
      pagina: this.paginaAtual,
      itensPorPagina: this.itensPorPagina
    });
  }
}
