using ApiBookCloud.Models;
using ApiBookCloud.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NugetModelsBookCloud.Models;

namespace ApiBookCloud.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly RepositoryWallet _repository;

        public WalletController(RepositoryWallet repository)
        {
            _repository = repository;
        }

        [Authorize]
        [HttpGet("saldo/{usuarioId}")]
        public async Task<ActionResult<decimal>> GetSaldo(int usuarioId)
        {
            var saldo = await _repository.GetSaldoUsuarioAsync(usuarioId);
            return Ok(saldo);
        }

        [Authorize]
        [HttpGet("movimientos/{usuarioId}")]
        public async Task<ActionResult<List<object>>> GetMovimientos(int usuarioId, [FromQuery] int limit = 20)
        {
            var movimientos = await _repository.GetMovimientosAsync(usuarioId, limit);
            
            // Devolver DTOs para evitar referencias circulares
            var dtos = movimientos.Select(m => new
            {
                m.Id,
                m.UsuarioId,
                m.PedidoId,
                m.Monto,
                m.Tipo,
                m.Descripcion,
                m.Fecha,
                m.Activo
            }).ToList();
            
            return Ok(dtos);
        }

        [Authorize]
        [HttpPost("recargar")]
        public async Task<ActionResult> RecargarSaldo([FromBody] RecargarSaldoRequestDto request)
        {
            try
            {
                await _repository.RecargarSaldoAsync(request.UsuarioId, request.Monto, request.Descripcion);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("descontar")]
        public async Task<ActionResult> DescontarSaldo([FromBody] DescontarSaldoRequestDto request)
        {
            try
            {
                await _repository.DescontarSaldoAsync(request.UsuarioId, request.PedidoId, request.Monto, request.Descripcion);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("tiene-saldo/{usuarioId}")]
        public async Task<ActionResult<bool>> TieneSaldoSuficiente(int usuarioId, [FromQuery] decimal monto)
        {
            var tiene = await _repository.TieneSaldoSuficienteAsync(usuarioId, monto);
            return Ok(tiene);
        }

        [Authorize]
        [HttpPost("transferir-vendedores")]
        public async Task<ActionResult> TransferirSaldoAVendedores([FromBody] TransferirSaldoRequestDto request)
        {
            try
            {
                await _repository.TransferirSaldoAVendedoresAsync(request.PedidoId, request.CompradorId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}