import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Produto, ItemCarrinho } from '../models/produto.model';

@Injectable({
  providedIn: 'root'
})
export class CarrinhoService {
  private itensCarrinho: ItemCarrinho[] = [];
  private carrinhoSubject = new BehaviorSubject<ItemCarrinho[]>([]);
  private quantidadeSubject = new BehaviorSubject<number>(0);

  carrinho$: Observable<ItemCarrinho[]> = this.carrinhoSubject.asObservable();
  quantidade$: Observable<number> = this.quantidadeSubject.asObservable();

  constructor() {
    this.carregarCarrinho();
  }

  private carregarCarrinho(): void {
    const carrinhoSalvo = localStorage.getItem('carrinho');
    if (carrinhoSalvo) {
      this.itensCarrinho = JSON.parse(carrinhoSalvo);
      this.atualizarCarrinho();
    }
  }

  private salvarCarrinho(): void {
    localStorage.setItem('carrinho', JSON.stringify(this.itensCarrinho));
  }

  private atualizarCarrinho(): void {
    this.carrinhoSubject.next([...this.itensCarrinho]);
    const quantidade = this.itensCarrinho.reduce((total, item) => total + item.quantidade, 0);
    this.quantidadeSubject.next(quantidade);
    this.salvarCarrinho();
  }

  adicionarProduto(produto: Produto, quantidade: number = 1, observacoes?: string, produtoVariacaoId?: string, tamanhoVariacao?: string): void {
    // Se é variação, verificar por variação. Senão por produto
    let itemExistente: ItemCarrinho | undefined;
    
    if (produtoVariacaoId) {
      itemExistente = this.itensCarrinho.find(item => 
        item.produto.id === produto.id && 
        item.produtoVariacaoId === produtoVariacaoId
      );
    } else {
      itemExistente = this.itensCarrinho.find(item => 
        item.produto.id === produto.id && 
        !item.produtoVariacaoId
      );
    }

    if (itemExistente) {
      itemExistente.quantidade += quantidade;
      if (observacoes) {
        itemExistente.observacoes = observacoes;
      }
    } else {
      this.itensCarrinho.push({
        produto,
        quantidade,
        observacoes,
        produtoVariacaoId,
        tamanhoVariacao
      });
    }

    this.atualizarCarrinho();
  }

  removerProduto(produtoId: string, produtoVariacaoId?: string): void {
    if (produtoVariacaoId) {
      this.itensCarrinho = this.itensCarrinho.filter(item => 
        !(item.produto.id === produtoId && item.produtoVariacaoId === produtoVariacaoId)
      );
    } else {
      this.itensCarrinho = this.itensCarrinho.filter(item => item.produto.id !== produtoId);
    }
    this.atualizarCarrinho();
  }

  atualizarQuantidade(produtoId: string, quantidade: number, produtoVariacaoId?: string): void {
    const item = produtoVariacaoId 
      ? this.itensCarrinho.find(item => 
          item.produto.id === produtoId && 
          item.produtoVariacaoId === produtoVariacaoId
        )
      : this.itensCarrinho.find(item => 
          item.produto.id === produtoId && 
          !item.produtoVariacaoId
        );
    
    if (item) {
      if (quantidade <= 0) {
        this.removerProduto(produtoId, produtoVariacaoId);
      } else {
        item.quantidade = quantidade;
        this.atualizarCarrinho();
      }
    }
  }

  limparCarrinho(): void {
    this.itensCarrinho = [];
    this.atualizarCarrinho();
  }

  obterItens(): ItemCarrinho[] {
    return [...this.itensCarrinho];
  }

  obterQuantidadeItens(): number {
    return this.itensCarrinho.reduce((total, item) => total + item.quantidade, 0);
  }

  calcularSubtotal(): number {
    return this.itensCarrinho.reduce((total, item) => {
      const preco = item.produto.precoPromocional || item.produto.preco;
      return total + (preco * item.quantidade);
    }, 0);
  }

  calcularTotal(frete: number = 0, desconto: number = 0): number {
    return this.calcularSubtotal() + frete - desconto;
  }
}
