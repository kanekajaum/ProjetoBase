using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoBase.Domain.Entities;
using ProjetoBase.Domain.Interfaces;
using ProjetoBase.Infrastructure.Services;

namespace ProjetoBase.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutoController : ControllerBase
    {
        private readonly IProdutoService _produtoAppService;

        public ProdutoController(IProdutoService produtoAppService)
        {
            _produtoAppService = produtoAppService;
        }

        [AllowAnonymous]
        [HttpGet("ListarTodos")]
        public async Task<IActionResult> ListarTodos()
        {
            var produtos = await _produtoAppService.GetAllAsync();
            return Ok(produtos);
        }

        [Authorize]
        [HttpGet("BuscarPorId")]
        public async Task<IActionResult> BuscarPorId(int produtoID)
        {
            var produto = await _produtoAppService.GetByIdAsync(produtoID);
            if (produto == null)
                return NotFound();

            return Ok(produto);
        }

        [Authorize]
        [HttpPost("Cadastrar")]
        public async Task<IActionResult> Cadastrar([FromBody] Produto produto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _produtoAppService.AddAsync(produto);
            return CreatedAtAction(nameof(BuscarPorId), new { id = produto.Id }, produto);
        }

        [Authorize]
        [HttpPut("AlterarProduto")]
        public async Task<IActionResult> AlterarProduto(int id, [FromBody] Produto produto)
        {
            var existente = await _produtoAppService.GetByIdAsync(id);
            if (existente == null)
                return NotFound();

            if (existente.Nome != produto.Nome)
            {
                existente.AtualizarNome(produto.Nome);
            }
            if (existente.Preco != produto.Preco)
            {
                existente.AtualizarPreco(produto.Preco);
            }

            await _produtoAppService.UpdateAsync(existente);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("Remover")]
        public async Task<IActionResult> Remover(int id)
        {
            var produto = await _produtoAppService.GetByIdAsync(id);
            if (produto == null)
                return NotFound();

            await _produtoAppService.DeleteAsync(produto.Id);
            return NoContent();
        }
    }
}