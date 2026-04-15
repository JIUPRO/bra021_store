SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    PRINT '1. Removendo movimentacoes de estoque ligadas a pedidos...';

    DELETE ME
    FROM MovimentacoesEstoque ME
    WHERE ME.Referencia IS NOT NULL
      AND (
            ME.Referencia LIKE 'PED%'
            OR ME.Motivo LIKE 'Venda - Pedido %'
            OR ME.Motivo LIKE 'Cancelamento - Pedido %'
          );

    PRINT '2. Recalculando estoque de ProdutoTamanhos...';

    ;WITH MovSaldo AS (
        SELECT
            me.ProdutoTamanhoId,
            SUM(
                CASE
                    WHEN me.Ativo = 0 THEN 0
                    WHEN me.Tipo IN (1, 4) THEN me.Quantidade
                    WHEN me.Tipo = 2 THEN -me.Quantidade
                    WHEN me.Tipo = 3 THEN 0
                    ELSE 0
                END
            ) AS SaldoMov
        FROM MovimentacoesEstoque me
        GROUP BY me.ProdutoTamanhoId
    )
    UPDATE pt
    SET
        pt.QuantidadeEstoque = ISNULL(ms.SaldoMov, 0),
        pt.DataAtualizacao = GETUTCDATE()
    FROM ProdutoTamanhos pt
    LEFT JOIN MovSaldo ms ON ms.ProdutoTamanhoId = pt.Id;

    PRINT '3. Removendo itens de pedido...';

    DELETE FROM ItensPedido;

    PRINT '4. Removendo pedidos...';

    DELETE FROM Pedidos;

    COMMIT TRANSACTION;

    PRINT 'LIMPEZA CONCLUIDA COM SUCESSO.';

    SELECT
        (SELECT COUNT(*) FROM Pedidos) AS TotalPedidos,
        (SELECT COUNT(*) FROM ItensPedido) AS TotalItensPedido,
        (SELECT COUNT(*) FROM MovimentacoesEstoque) AS TotalMovimentacoesEstoque,
        (SELECT COUNT(*) FROM ProdutoTamanhos) AS TotalProdutoTamanhos;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT 'ERRO NA LIMPEZA. TRANSACAO DESFEITA.';
    THROW;
END CATCH;
