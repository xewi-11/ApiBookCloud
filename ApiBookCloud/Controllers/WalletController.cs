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
        public async Task<ActionResult<List<SaldoMovimiento>>> GetMovimientos(int usuarioId, [FromQuery] int limit = 20)
        {
            var movimientos = await _repository.GetMovimientosAsync(usuarioId, limit);
            return Ok(movimientos);
        }

        [Authorize]
        [HttpPost("recargar")]
        public async Task<ActionResult> RecargarSaldo(int usuarioId, [FromQuery] decimal monto, [FromQuery] string descripcion)
        {
            try
            {
                await _repository.RecargarSaldoAsync(usuarioId, monto, descripcion);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("descontar")]
        public async Task<ActionResult> DescontarSaldo(int usuarioId, [FromQuery] int pedidoId, [FromQuery] decimal monto, [FromQuery] string descripcion)
        {
            try
            {
                await _repository.DescontarSaldoAsync(usuarioId, pedidoId, monto, descripcion);
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
        public async Task<ActionResult> TransferirSaldoAVendedores([FromQuery] int pedidoId, [FromQuery] int compradorId)
        {
            try
            {
                await _repository.TransferirSaldoAVendedoresAsync(pedidoId, compradorId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}