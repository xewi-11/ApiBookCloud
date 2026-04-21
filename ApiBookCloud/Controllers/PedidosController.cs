using ApiBookCloud.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NugetModelsBookCloud.Models;

namespace ApiBookCloud.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController : ControllerBase
    {
        private readonly RepositoryPedidos _repository;

        public PedidosController(RepositoryPedidos repository)
        {
            _repository = repository;
        }

        [Authorize]
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<List<Pedido>>> GetPedidosUsuario(int usuarioId)
        {
            var pedidos = await _repository.GetPedidosUsuarioAsync(usuarioId);
            return Ok(pedidos);
        }

        [Authorize]
        [HttpGet("{pedidoId}")]
        public async Task<ActionResult<Pedido>> GetPedido(int pedidoId)
        {
            var pedido = await _repository.GetPedidoAsync(pedidoId);
            if (pedido == null)
            {
                return NotFound();
            }
            return Ok(pedido);
        }

        [Authorize]
        [HttpPost("crear")]
        public async Task<ActionResult<int>> CrearPedido(int usuarioId, decimal total, [FromBody] List<PedidoDetalle> detalles)
        {
            try
            {
                int pedidoId = await _repository.CrearPedidoAsync(usuarioId, total, detalles);
                return Ok(pedidoId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("estado/{pedidoId}")]
        public async Task<ActionResult> ActualizarEstado(int pedidoId, [FromBody] string estado)
        {
            await _repository.ActualizarEstadoPedidoAsync(pedidoId, estado);
            return Ok();
        }
    }
}