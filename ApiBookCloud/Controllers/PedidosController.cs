using ApiBookCloud.Models;
using ApiBookCloud.Repositories;
using ApiOAuthEmpleados.Helpers;
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
        private readonly HelperUsuarioToken _helperUsuarioToken;

        public PedidosController(RepositoryPedidos repository, HelperUsuarioToken helperUsuarioToken)
        {
            _repository = repository;
            _helperUsuarioToken = helperUsuarioToken;
        }

        [Authorize]
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<List<object>>> GetPedidosUsuario(int usuarioId)
        {
            var pedidos = await _repository.GetPedidosUsuarioAsync(usuarioId);
            
            // Devolver DTOs para evitar referencias circulares
            var dtos = pedidos.Select(p => new
            {
                p.Id,
                p.UsuarioId,
                p.FechaPedido,
                p.Total,
                p.Estado,
                p.Activo
            }).ToList();
            
            return Ok(dtos);
        }

        [Authorize]
        [HttpGet("{pedidoId}")]
        public async Task<ActionResult<object>> GetPedido(int pedidoId)
        {
            var pedido = await _repository.GetPedidoAsync(pedidoId);
            if (pedido == null)
            {
                return NotFound();
            }
            
            // Devolver DTO con detalles pero sin referencias circulares
            var dto = new
            {
                pedido.Id,
                pedido.UsuarioId,
                pedido.FechaPedido,
                pedido.Total,
                pedido.Estado,
                pedido.Activo,
                Detalles = pedido.PedidoDetalles?.Where(d => d.Activo).Select(d => new
                {
                    d.Id,
                    d.PedidoId,
                    d.LibroId,
                    d.Cantidad,
                    d.PrecioUnitario,
                    LibroTitulo = d.Libro?.Titulo,
                    LibroAutor = d.Libro?.Autor,
                    LibroFoto = d.Libro?.Foto,
                    d.Activo
                }).ToList()
            };
            
            return Ok(dto);
        }

        [Authorize]
        [HttpPost("crear")]
        public async Task<ActionResult<int>> CrearPedido([FromBody] CrearPedidoRequestDto request)
        {
            try
            {
                int? usuarioId = _helperUsuarioToken.GetUserId();
                if (!usuarioId.HasValue)
                {
                    return Unauthorized("No se pudo identificar al usuario autenticado.");
                }

                List<PedidoDetalle> detalles = request.Detalles.Select(d => new PedidoDetalle
                {
                    LibroId = d.LibroId,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Activo = true
                }).ToList();

                int pedidoId = await _repository.CrearPedidoAsync(usuarioId.Value, request.Total, detalles);
                return Ok(pedidoId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("estado/{pedidoId}")]
        public async Task<ActionResult> ActualizarEstado(int pedidoId, [FromBody] ActualizarEstadoPedidoRequestDto request)
        {
            await _repository.ActualizarEstadoPedidoAsync(pedidoId, request.Estado);
            return Ok();
        }
    }
}