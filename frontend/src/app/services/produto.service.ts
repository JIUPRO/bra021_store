import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Produto, Categoria } from '../models/produto.model';

@Injectable({
  providedIn: 'root'
})
export class ProdutoService {
  constructor(private api: ApiService) {}

  obterTodos(): Observable<Produto[]> {
    return this.api.get<Produto[]>('produtos');
  }

  obterDestaques(): Observable<Produto[]> {
    return this.api.get<Produto[]>('produtos/destaques');
  }

  obterPorId(id: string): Observable<Produto> {
    return this.api.getById<Produto>('produtos', id);
  }

  obterPorCategoria(categoriaId: string): Observable<Produto[]> {
    return this.api.get<Produto[]>(`produtos/categoria/${categoriaId}`);
  }

  pesquisar(termo: string): Observable<Produto[]> {
    return this.api.get<Produto[]>(`produtos/pesquisar?termo=${termo}`);
  }

  obterCategorias(): Observable<Categoria[]> {
    return this.api.get<Categoria[]>('categorias');
  }

  obterCategoriaPorId(id: string): Observable<Categoria> {
    return this.api.getById<Categoria>('categorias', id);
  }
}
