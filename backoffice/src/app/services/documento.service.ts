import { Injectable, inject } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { BehaviorSubject } from 'rxjs';

export interface DocumentoVisualizacao {
  titulo: string;
  nomeArquivo: string;
  blob: Blob;
  objectUrl: string;
  safeUrl: SafeResourceUrl;
}

@Injectable({
  providedIn: 'root'
})
export class DocumentoService {
  private sanitizer = inject(DomSanitizer);
  private documentoSubject = new BehaviorSubject<DocumentoVisualizacao | null>(null);

  readonly documento$ = this.documentoSubject.asObservable();

  exibirDocumento(blob: Blob, nomeArquivo: string, titulo = 'Visualizar documento'): void {
    this.fecharDocumentoAtual();

    const objectUrl = URL.createObjectURL(blob);

    this.documentoSubject.next({
      titulo,
      nomeArquivo,
      blob,
      objectUrl,
      safeUrl: this.sanitizer.bypassSecurityTrustResourceUrl(objectUrl)
    });
  }

  fechar(): void {
    this.fecharDocumentoAtual();
    this.documentoSubject.next(null);
  }

  baixar(documento: DocumentoVisualizacao): void {
    const link = document.createElement('a');
    link.href = documento.objectUrl;
    link.download = documento.nomeArquivo;
    link.click();
  }

  private fecharDocumentoAtual(): void {
    const atual = this.documentoSubject.value;
    if (atual?.objectUrl) {
      URL.revokeObjectURL(atual.objectUrl);
    }
  }
}
