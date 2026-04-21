using ApiBookCloud.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NugetModelsBookCloud.Models;

namespace ApiBookCloud.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagosController : ControllerBase
    {
        private readonly RepositoryPagos _repository;

        public PagosController(RepositoryPagos repository)
        {
            _repository = repository;
        }

        [Authorize]
        [HttpPost("crear")]
        public async Task<ActionResult<int>> CrearPago([FromBody] Pago pago)
        {
            var id = await _repository.CrearPagoAsync(pago);
            return Ok(id);
        }

        [Authorize]
        [HttpGet("{pagoId}")]
        public async Task<ActionResult<Pago>> GetPago(int pagoId)
        {
            var pago = await _repository.GetPagoAsync(pagoId);
            if (pago == null) return NotFound();
            return Ok(pago);
        }

        [Authorize]
        [HttpGet("pedido/{pedidoId}")]
        public async Task<ActionResult<List<Pago>>> GetPagosPorPedido(int pedidoId)
        {
            var pagos = await _repository.GetPagosPorPedidoAsync(pedidoId);
            return Ok(pagos);
        }

        [Authorize]
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<List<Pago>>> GetPagosPorUsuario(int usuarioId)
        {
            var pagos = await _repository.GetPagosPorUsuarioAsync(usuarioId);
            return Ok(pagos);
        }

        [Authorize]
        [HttpPost("stripe/insertar")]
        public async Task<ActionResult> InsertarStripePago([FromBody] StripePago stripePago)
        {
            await _repository.InsertarStripePagoAsync(stripePago);
            return Ok();
        }

        [Authorize]
        [HttpGet("stripe/{sessionId}")]
        public async Task<ActionResult<StripePago>> GetStripePagoBySessionId(string sessionId)
        {
            var stripePago = await _repository.GetStripePagoBySessionIdAsync(sessionId);
            if (stripePago == null) return NotFound();
            return Ok(stripePago);
        }

        [Authorize]
        [HttpPut("stripe/estado/{sessionId}")]
        public async Task<ActionResult> ActualizarEstadoStripe(string sessionId, [FromBody] string nuevoEstado)
        {
            await _repository.ActualizarEstadoStripeAsync(sessionId, nuevoEstado);
            return Ok();
        }
    }
}