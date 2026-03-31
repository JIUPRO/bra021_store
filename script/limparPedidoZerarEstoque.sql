BEGIN TRY
    BEGIN TRANSACTION;

    -- 1. Apaga movimentações de estoque relacionadas a pedidos
    DELETE ME
    FROM MovimentacoesEstoque ME
    WHERE ME.Referencia IS NOT NULL
      AND (
            ME.Referencia LIKE 'PED%'
            OR ME.Motivo LIKE 'Venda - Pedido %'
            OR ME.Motivo LIKE 'Cancelamento - Pedido %'
          );

    -- 2. Recalcula o estoque dos tamanhos com base apenas nas movimentações restantes
    ;WITH MovSaldo AS (
        SELECT
            me.ProdutoTamanhoId,
            SUM(
                CASE
                    WHEN me.Ativo = 0 THEN 0
                    WHEN me.Tipo IN (1, 4) THEN me.Quantidade      -- Entrada, Devolucao
                    WHEN me.Tipo = 2 THEN -me.Quantidade           -- Saida
                    WHEN me.Tipo = 3 THEN 0                        -- Ajuste nao é delta confiável
                    ELSE 0
                END
            ) AS SaldoMov
        FROM MovimentacoesEstoque me
        GROUP BY me.ProdutoTamanhoId
    )
    UPDATE pt
    SET pt.QuantidadeEstoque = ISNULL(ms.SaldoMov, 0),
        pt.DataAtualizacao = GETUTCDATE()
    FROM ProdutoTamanhos pt
    LEFT JOIN MovSaldo ms ON ms.ProdutoTamanhoId = pt.Id;

    -- 3. Apaga itens de pedido
    DELETE FROM ItensPedido;

    -- 4. Apaga pedidos
    DELETE FROM Pedidos;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
