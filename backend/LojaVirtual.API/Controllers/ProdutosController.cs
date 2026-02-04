using Microsoft.AspNetCore.Mvc;
using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Aplicacao.Services;

namespace LojaVirtual.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutosController : ControllerBase
    {
        private readonly IProdutoService _produtoService;

        public ProdutosController(IProdutoService servicoProduto)
        {
            _produtoService = servicoProduto;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> ObterTodos()
        {
            var produtos = await _produtoService.ObterTodosAsync();
            return Ok(produtos);
        }

        [HttpGet("destaques")]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> ObterDestaques()
        {
            var produtos = await _produtoService.ObterProdutosEmDestaqueAsync();
            return Ok(produtos);
        }

        [HttpGet("categoria/{categoriaId}")]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> ObterPorCategoria(Guid categoriaId)
        {
            var produtos = await _produtoService.ObterPorCategoriaAsync(categoriaId);
            return Ok(produtos);
        }

        [HttpGet("pesquisar")]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> Pesquisar([FromQuery] string termo)
        {
            var produtos = await _produtoService.PesquisarAsync(termo);
            return Ok(produtos);
        }

        [HttpGet("estoque-baixo")]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> ObterEstoqueBaixo()
        {
            var produtos = await _produtoService.ObterComEstoqueBaixoAsync();
            return Ok(produtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProdutoDTO>> ObterPorId(Guid id)
        {
            var produto = await _produtoService.ObterPorIdAsync(id);
            if (produto == null)
                return NotFound(new { mensagem = "Produto não encontrado" });

            return Ok(produto);
        }

        [HttpPost]
        public async Task<ActionResult<ProdutoDTO>> Criar([FromBody] CriarProdutoDTO dto)
        {
            try
            {
                var produto = await _produtoService.CriarAsync(dto);
                return CreatedAtAction(nameof(ObterPorId), new { id = produto.Id }, produto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProdutoDTO>> Atualizar(Guid id, [FromBody] AtualizarProdutoDTO dto)
        {
            if (id != dto.Id)
                return BadRequest(new { mensagem = "ID da URL não corresponde ao ID do corpo" });

            try
            {
                var produto = await _produtoService.AtualizarAsync(dto);
                if (produto == null)
                    return NotFound(new { mensagem = "Produto não encontrado" });

                return Ok(produto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Remover(Guid id)
        {
            var resultado = await _produtoService.RemoverAsync(id);
            if (!resultado)
                return NotFound(new { mensagem = "Produto não encontrado" });

            return NoContent();
        }
    }
}
