import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

declare const MercadoPago: any;

export interface CriarPagamentoRequest {
  pedidoId: string;
  metodoPagamento: string;
  parcelas?: number;  // ← NOVO
  dadosCartao?: {
    cardNumber: string;
    cardholderName: string;
    expirationMonth: string;
    expirationYear: string;
    securityCode: string;
    cardToken?: string;
  };
}

export interface PagamentoResponse {
  sucesso: boolean;
  pagamentoId?: string;
  status?: string;
  statusDetalhe?: string;
  qrCodeBase64?: string;
  qrCode?: string;
  ticketUrl?: string;
  mensagem?: string;
  parcelas?: number;           // ← NOVO
  valorParcela?: number;       // ← NOVO
  codigoErro?: string;         // ← NOVO
}

@Injectable({
  providedIn: 'root'
})
export class PagamentoService {
  private apiUrl = `${environment.apiUrl}/pagamentos`;
  private mercadoPago: any;

  constructor(private http: HttpClient) {
    this.inicializarMercadoPago();
  }

  private async inicializarMercadoPago(): Promise<void> {
    // Carregar script do Mercado Pago
    const script = document.createElement('script');
    script.src = 'https://sdk.mercadopago.com/js/v2';
    script.async = true;
    
    await new Promise((resolve, reject) => {
      script.onload = resolve;
      script.onerror = reject;
      document.head.appendChild(script);
    });

    // Inicializar com a Public Key
    this.mercadoPago = new MercadoPago(environment.mercadoPagoPublicKey, {
      locale: 'pt-BR'
    });
  }

  async criarTokenCartao(dadosCartao: any): Promise<string> {
    try {
      // Aguardar inicialização do MercadoPago
      if (!this.mercadoPago) {
        console.log('MercadoPago não inicializado, aguardando...');
        await this.inicializarMercadoPago();
      }
      
      console.log('Criando token com dados:', {
        cardNumber: dadosCartao.cardNumber?.substring(0, 6) + '****',
        cardholderName: dadosCartao.cardholderName,
        expirationMonth: dadosCartao.expirationMonth,
        expirationYear: dadosCartao.expirationYear,
        cpf: dadosCartao.cpf?.replace(/\d(?=\d{2})/g, '*')
      });

      const token = await this.mercadoPago.createCardToken({
        cardNumber: dadosCartao.cardNumber.replace(/\s/g, ''),
        cardholderName: dadosCartao.cardholderName,
        cardExpirationMonth: dadosCartao.expirationMonth,
        cardExpirationYear: dadosCartao.expirationYear,
        securityCode: dadosCartao.securityCode,
        identificationType: 'CPF',
        identificationNumber: dadosCartao.cpf?.replace(/\D/g, '')
      });

      console.log('Token criado com sucesso:', token.id);
      return token.id;
    } catch (error) {
      console.error('Erro ao criar token do cartão:', error);
      throw error;
    }
  }

  criarPagamento(request: CriarPagamentoRequest): Observable<PagamentoResponse> {
    return this.http.post<PagamentoResponse>(`${this.apiUrl}/criar`, request);
  }

  getMercadoPago() {
    return this.mercadoPago;
  }
}
