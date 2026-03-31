import { CommonModule } from '@angular/common';
import { Component, ElementRef, OnDestroy, ViewChild, inject } from '@angular/core';
import { DocumentoService, DocumentoVisualizacao } from '../../services/documento.service';

@Component({
  selector: 'app-documento-visualizador',
  standalone: true,
  imports: [CommonModule],
  template: `
    <ng-container *ngIf="documentoService.documento$ | async as documento">
      <div class="documento-backdrop" (click)="fechar()"></div>

      <div class="documento-modal" role="dialog" aria-modal="true" aria-label="Visualizador de documento">
        <div class="documento-header">
          <div>
            <h5 class="mb-1">{{ documento.titulo }}</h5>
            <small class="text-muted">{{ documento.nomeArquivo }}</small>
          </div>

          <div class="documento-acoes">
            <button type="button" class="btn btn-outline-success btn-sm" (click)="imprimir()">
              <i class="bi bi-printer me-1"></i>Imprimir
            </button>
            <button type="button" class="btn btn-success btn-sm" (click)="baixar(documento)">
              <i class="bi bi-download me-1"></i>Salvar
            </button>
            <button type="button" class="btn btn-outline-secondary btn-sm" (click)="fechar()">
              <i class="bi bi-x-lg"></i>
            </button>
          </div>
        </div>

        <div class="documento-body">
          <iframe
            #iframeDocumento
            [src]="documento.safeUrl"
            title="Documento PDF"
            class="documento-frame">
          </iframe>
        </div>
      </div>
    </ng-container>
  `,
  styles: [`
    .documento-backdrop {
      position: fixed;
      inset: 0;
      background: rgba(15, 23, 42, 0.45);
      backdrop-filter: blur(2px);
      z-index: 1080;
    }

    .documento-modal {
      position: fixed;
      inset: 24px;
      z-index: 1090;
      display: flex;
      flex-direction: column;
      background: #fff;
      border-radius: 18px;
      overflow: hidden;
      box-shadow: 0 24px 80px rgba(15, 23, 42, 0.24);
    }

    .documento-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 16px;
      padding: 18px 20px;
      border-bottom: 1px solid #e5e7eb;
      background: linear-gradient(135deg, #f0fdf4 0%, #ffffff 100%);
    }

    .documento-acoes {
      display: flex;
      gap: 8px;
      align-items: center;
      flex-wrap: wrap;
    }

    .documento-body {
      flex: 1;
      background: #eef2f7;
      padding: 16px;
      min-height: 0;
    }

    .documento-frame {
      width: 100%;
      height: 100%;
      min-height: 420px;
      border: 0;
      border-radius: 12px;
      background: #fff;
    }

    @media (max-width: 768px) {
      .documento-modal {
        inset: 12px;
      }

      .documento-header {
        flex-direction: column;
        align-items: stretch;
      }

      .documento-acoes {
        justify-content: flex-end;
      }
    }
  `]
})
export class DocumentoVisualizadorComponent implements OnDestroy {
  readonly documentoService = inject(DocumentoService);

  @ViewChild('iframeDocumento')
  iframeDocumento?: ElementRef<HTMLIFrameElement>;

  fechar(): void {
    this.documentoService.fechar();
  }

  baixar(documento: DocumentoVisualizacao): void {
    this.documentoService.baixar(documento);
  }

  imprimir(): void {
    const iframeWindow = this.iframeDocumento?.nativeElement.contentWindow;
    if (!iframeWindow) {
      return;
    }

    iframeWindow.focus();
    iframeWindow.print();
  }

  ngOnDestroy(): void {
    this.documentoService.fechar();
  }
}
