import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface ParametroSistema {
  id: string;
  chave: string;
  valor: string;
  descricao?: string;
  tipo: string;
}

@Injectable({ providedIn: 'root' })
export class ParametroSistemaService {
  private api = inject(ApiService);

  obterPorChave(chave: string): Observable<ParametroSistema> {
    return this.api.get<ParametroSistema>(`parametrossistema/chave/${chave}`);
  }
}
