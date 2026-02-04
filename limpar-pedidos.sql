-- Restaurar estoque dos produtos
UPDATE pt
SET pt.QuantidadeEstoque = pt.QuantidadeEstoque + ISNULL(ip.QuantidadeDevolvida, 0)
FROM ProdutoTamanhos pt
	INNER JOIN (
		SELECT ip.ProdutoTamanhoId,
			SUM(ip.Quantidade) as QuantidadeDevolvida
		FROM ItensPedido ip
		WHERE ip.ProdutoTamanhoId IS NOT NULL
		GROUP BY ip.ProdutoTamanhoId
	) ip ON pt.Id = ip.ProdutoTamanhoId;
-- Deletar movimentações de estoque
DELETE FROM MovimentacoesEstoque
WHERE PedidoId IS NOT NULL;
-- Deletar itens de pedido
DELETE FROM ItensPedido;
-- Deletar pedidos
DELETE FROM Pedidos;
SELECT 'Limpeza concluída' as Status;