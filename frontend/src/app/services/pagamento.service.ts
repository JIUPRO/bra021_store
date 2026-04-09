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
    cardholderName: string;
    cpf?: string;
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

interface SecureCardFieldsContext {
  cardNumber: any;
  expirationDate: any;
  securityCode: any;
}

@Injectable({
  providedIn: 'root'
})
export class PagamentoService {
  private apiUrl = `${environment.apiUrl}/pagamentos`;
  private mercadoPago: any;
  private secureFields = new Map<string, SecureCardFieldsContext>();

  constructor(private http: HttpClient) {
    this.inicializarMercadoPago();
  }

  private async inicializarMercadoPago(): Promise<void> {
    console.log('Inicializando SDK Mercado Pago:', {
      origin: window.location.origin,
      href: window.location.href,
      production: environment.production,
      publicKeyPrefix: environment.mercadoPagoPublicKey?.substring(0, 18)
    });

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

    console.log('SDK Mercado Pago inicializado com sucesso.');
  }

  async criarTokenCartao(dadosCartao: any): Promise<string> {
    try {
      // Aguardar inicialização do MercadoPago
      if (!this.mercadoPago) {
        console.log('MercadoPago não inicializado, aguardando...');
        await this.inicializarMercadoPago();
      }

      const tentativas = this.montarTentativasTokenizacao(dadosCartao);

      console.log('Criando token com dados:', {
        cardNumber: tentativas[0].cardNumber?.substring(0, 6) + '****',
        cardholderName: tentativas[0].cardholderName,
        expirationMonth: tentativas[0].cardExpirationMonth,
        expirationYear: tentativas[0].cardExpirationYear,
        cpf: tentativas[0].identificationNumber?.replace(/\d(?=\d{2})/g, '*')
      });

      let ultimoErro: any = null;

      for (const tentativa of tentativas) {
        try {
          console.log('Tentando tokenizar cartão com formato:', {
            cardNumber: tentativa.cardNumber?.substring(0, 6) + '****',
            cardholderName: tentativa.cardholderName,
            cardExpirationMonth: tentativa.cardExpirationMonth,
            cardExpirationYear: tentativa.cardExpirationYear,
            identificationType: tentativa.identificationType,
            identificationNumber: tentativa.identificationNumber?.replace(/\d(?=\d{2})/g, '*')
          });

          const token = await this.mercadoPago.createCardToken(tentativa);

          console.log('Retorno bruto do Mercado Pago ao tokenizar:', token);

          if (token?.id) {
            console.log('Token criado com sucesso:', token.id);
            return token.id;
          }

          ultimoErro = token;
        } catch (error) {
          console.error('Falha em tentativa de tokenização:', {
            origin: window.location.origin,
            publicKeyPrefix: environment.mercadoPagoPublicKey?.substring(0, 18),
            error
          });
          ultimoErro = error;
        }
      }

      throw new Error(this.extrairMensagemErroMercadoPago(ultimoErro));
    } catch (error) {
      console.error('Erro ao criar token do cartão:', error);
      throw error;
    }
  }

  criarPagamento(request: CriarPagamentoRequest): Observable<PagamentoResponse> {
    return this.http.post<PagamentoResponse>(`${this.apiUrl}/criar`, request);
  }

  async inicializarCamposCartaoSeguro(prefixo: string): Promise<void> {
    if (!this.mercadoPago) {
      await this.inicializarMercadoPago();
    }

    if (!this.mercadoPago?.fields?.create) {
      throw new Error('SDK do Mercado Pago não expôs MercadoPago.fields.create.');
    }

    this.destruirCamposCartaoSeguro(prefixo);

    const estilosBase = {
      fontSize: '16px',
      color: '#212529'
    };

    const cardNumber = this.mercadoPago.fields.create('cardNumber', {
      placeholder: '0000 0000 0000 0000',
      style: estilosBase
    });
    cardNumber.mount(`${prefixo}-card-number`);

    const expirationDate = this.mercadoPago.fields.create('expirationDate', {
      placeholder: 'MM/AA',
      style: estilosBase
    });
    expirationDate.mount(`${prefixo}-expiration-date`);

    const securityCode = this.mercadoPago.fields.create('securityCode', {
      placeholder: '123',
      style: estilosBase
    });
    securityCode.mount(`${prefixo}-security-code`);

    this.secureFields.set(prefixo, {
      cardNumber,
      expirationDate,
      securityCode
    });

    console.log('Campos seguros do Mercado Pago montados:', {
      prefixo,
      origin: window.location.origin,
      publicKeyPrefix: environment.mercadoPagoPublicKey?.substring(0, 18)
    });
  }

  async criarTokenCartaoSeguro(prefixo: string, dadosTitular: { cardholderName: string; cpf: string }): Promise<string> {
    try {
      if (!this.mercadoPago) {
        await this.inicializarMercadoPago();
      }

      if (!this.secureFields.has(prefixo)) {
        throw new Error('Os campos seguros do cartão não foram inicializados.');
      }

      const cardholderName = (dadosTitular.cardholderName || '').trim().replace(/\s+/g, ' ');
      const identificationNumber = (dadosTitular.cpf || '').replace(/\D/g, '');

      console.log('Criando token via Secure Fields:', {
        prefixo,
        cardholderName,
        cpf: identificationNumber.replace(/\d(?=\d{2})/g, '*')
      });

      const token = await this.mercadoPago.fields.createCardToken({
        cardholderName,
        identificationType: 'CPF',
        identificationNumber
      });

      console.log('Retorno bruto Secure Fields:', token);

      if (token?.id) {
        return token.id;
      }

      throw new Error(this.extrairMensagemErroMercadoPago(token));
    } catch (error) {
      console.error('Erro ao criar token do cartão com Secure Fields:', error);
      throw error;
    }
  }

  destruirCamposCartaoSeguro(prefixo: string): void {
    const context = this.secureFields.get(prefixo);
    if (!context) {
      return;
    }

    try { context.cardNumber?.unmount?.(); } catch {}
    try { context.expirationDate?.unmount?.(); } catch {}
    try { context.securityCode?.unmount?.(); } catch {}

    this.secureFields.delete(prefixo);
  }

  getMercadoPago() {
    return this.mercadoPago;
  }

  private montarTentativasTokenizacao(dadosCartao: any): any[] {
    const numeroCartao = (dadosCartao.cardNumber || '').replace(/\D/g, '');
    const nomeTitular = (dadosCartao.cardholderName || '')
      .trim()
      .replace(/\s+/g, ' ');
    const mes = (dadosCartao.expirationMonth || '').replace(/\D/g, '').padStart(2, '0').slice(-2);
    const anoNumerico = (dadosCartao.expirationYear || '').replace(/\D/g, '');
    const cvv = (dadosCartao.securityCode || '').replace(/\D/g, '');
    const cpf = (dadosCartao.cpf || '').replace(/\D/g, '');

    const anos = Array.from(new Set([
      anoNumerico.length === 4 ? anoNumerico : '',
      anoNumerico.length === 2 ? `20${anoNumerico}` : '',
      anoNumerico.length >= 2 ? anoNumerico.slice(-2) : ''
    ].filter(Boolean)));

    return anos.map(ano => ({
      cardNumber: numeroCartao,
      cardholderName: nomeTitular,
      cardExpirationMonth: mes,
      cardExpirationYear: ano,
      securityCode: cvv,
      identificationType: 'CPF',
      identificationNumber: cpf
    }));
  }

  private extrairMensagemErroMercadoPago(error: any): string {
    console.error('Payload bruto de erro do Mercado Pago:', error);

    if (!error) {
      console.error('Diagnóstico: o SDK não devolveu payload de erro. Verifique CORS/origem, bloqueio de navegador e a public key ativa.');
    }

    const causaArray = Array.isArray(error?.cause) ? error.cause : [];
    const causa = error?.cause?.[0];

    return (
      error?.message?.message ||
      error?.message?.error ||
      causa?.description ||
      causa?.message ||
      causaArray.map((item: any) => item?.description || item?.message).filter(Boolean).join(' | ') ||
      error?.message ||
      error?.error?.message ||
      'Não foi possível validar os dados do cartão.'
    );
  }
}
