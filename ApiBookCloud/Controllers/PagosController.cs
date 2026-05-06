using ApiBookCloud.Models;
using ApiBookCloud.Repositories;
using ApiOAuthEmpleados.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NugetModelsBookCloud.Models;
using Stripe;
using Stripe.Checkout;

namespace ApiBookCloud.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagosController : ControllerBase
    {
        private readonly RepositoryPagos _repository;
        private readonly RepositoryPedidos _repositoryPedidos;
        private readonly HelperUsuarioToken _helperUsuarioToken;
        private readonly IConfiguration _configuration;

        public PagosController(
            RepositoryPagos repository,
            RepositoryPedidos repositoryPedidos,
            HelperUsuarioToken helperUsuarioToken,
            IConfiguration configuration)
        {
            _repository = repository;
            _repositoryPedidos = repositoryPedidos;
            _helperUsuarioToken = helperUsuarioToken;
            _configuration = configuration;
        }

        [Authorize]
        [HttpPost("crear")]
        public async Task<ActionResult<int>> CrearPago([FromBody] CrearPagoRequestDto request)
        {
            Pago pago = new Pago
            {
                PedidoId = request.PedidoId,
                FechaPago = request.FechaPago,
                Monto = request.Monto,
                Metodo = request.Metodo,
                Estado = request.Estado,
                Activo = request.Activo
            };

            var id = await _repository.CrearPagoAsync(pago);
            return Ok(id);
        }

        [Authorize]
        [HttpPost("stripe/crear-sesion-checkout")]
        public async Task<ActionResult<StripeCheckoutCreateResponseDto>> CrearSesionCheckout([FromBody] StripeCheckoutCreateRequestDto request)
        {
            int? usuarioId = _helperUsuarioToken.GetUserId();
            if (!usuarioId.HasValue)
            {
                return Unauthorized("No se pudo identificar al usuario autenticado.");
            }

            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
            if (string.IsNullOrWhiteSpace(StripeConfiguration.ApiKey))
            {
                return BadRequest("Falta configurar Stripe:SecretKey en la API.");
            }

            // Prefer an explicit frontend base URL when configured so Stripe redirects to the UI
            string frontBase = _configuration["FrontBaseUrl"];
            string domain = !string.IsNullOrWhiteSpace(frontBase) ? frontBase.TrimEnd('/') + "/" : $"{Request.Scheme}://{Request.Host}/";

            SessionCreateOptions options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + "Stripe/PagoExitoso?sessionId={CHECKOUT_SESSION_ID}",
                CancelUrl = domain + "Stripe/PagoCancelado?sessionId={CHECKOUT_SESSION_ID}"
            };

            int? pedidoId = null;
            decimal totalFinal = 0;

            if (request.TipoOperacion == "COMPRA")
            {
                List<PedidoDetalle> detalles = request.Detalles.Select(d => new PedidoDetalle
                {
                    LibroId = d.LibroId,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Activo = true
                }).ToList();

                totalFinal = request.Total ?? detalles.Sum(d => d.PrecioUnitario * d.Cantidad);
                pedidoId = await _repositoryPedidos.CrearPedidoAsync(usuarioId.Value, totalFinal, detalles);

                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(totalFinal * 100),
                        Currency = "eur",
                        ProductData = new SessionLineItemPriceDataProductDataOptions { Name = "Compra de Libros en BookCloud" }
                    },
                    Quantity = 1
                });
            }
            else
            {
                totalFinal = request.MontoRecarga ?? 0;
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(totalFinal * 100),
                        Currency = "eur",
                        ProductData = new SessionLineItemPriceDataProductDataOptions { Name = "Recarga de Wallet BookCloud" }
                    },
                    Quantity = 1
                });
            }

            SessionService service = new SessionService();
            Session session = await service.CreateAsync(options);

            StripePago stripePago = new StripePago
            {
                UsuarioId = usuarioId.Value,
                PedidoId = pedidoId,
                StripeSessionId = session.Id,
                Monto = totalFinal,
                TipoOperacion = request.TipoOperacion,
                Estado = "pending"
            };

            await _repository.InsertarStripePagoAsync(stripePago);

            return Ok(new StripeCheckoutCreateResponseDto
            {
                SessionId = session.Id,
                CheckoutUrl = session.Url,
                PedidoId = pedidoId
            });
        }

        [Authorize]
        [HttpGet("{pagoId}")]
        public async Task<ActionResult<object>> GetPago(int pagoId)
        {
            var pago = await _repository.GetPagoAsync(pagoId);
            if (pago == null) return NotFound();
            
            // Devolver DTO para evitar referencias circulares
            var dto = new
            {
                pago.Id,
                pago.PedidoId,
                pago.FechaPago,
                pago.Monto,
                pago.Metodo,
                pago.Estado,
                pago.Activo
            };
            return Ok(dto);
        }

        [Authorize]
        [HttpGet("pedido/{pedidoId}")]
        public async Task<ActionResult<List<object>>> GetPagosPorPedido(int pedidoId)
        {
            var pagos = await _repository.GetPagosPorPedidoAsync(pedidoId);
            
            // Devolver DTOs para evitar referencias circulares
            var dtos = pagos.Select(p => new
            {
                p.Id,
                p.PedidoId,
                p.FechaPago,
                p.Monto,
                p.Metodo,
                p.Estado,
                p.Activo
            }).ToList();
            
            return Ok(dtos);
        }

        [Authorize]
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<List<object>>> GetPagosPorUsuario(int usuarioId)
        {
            var pagos = await _repository.GetPagosPorUsuarioAsync(usuarioId);
            
            // Devolver DTOs para evitar referencias circulares
            var dtos = pagos.Select(p => new
            {
                p.Id,
                p.PedidoId,
                p.FechaPago,
                p.Monto,
                p.Metodo,
                p.Estado,
                p.Activo
            }).ToList();
            
            return Ok(dtos);
        }

        [Authorize]
        [HttpPost("stripe/insertar")]
        public async Task<ActionResult> InsertarStripePago([FromBody] InsertarStripePagoRequestDto request)
        {
            StripePago stripePago = new StripePago
            {
                UsuarioId = request.UsuarioId,
                PedidoId = request.PedidoId,
                StripeSessionId = request.StripeSessionId,
                Monto = request.Monto,
                TipoOperacion = request.TipoOperacion,
                Estado = request.Estado
            };

            await _repository.InsertarStripePagoAsync(stripePago);
            return Ok();
        }

        [Authorize]
        [HttpGet("stripe/{sessionId}")]
        public async Task<ActionResult<object>> GetStripePagoBySessionId(string sessionId)
        {
            var stripePago = await _repository.GetStripePagoBySessionIdAsync(sessionId);
            if (stripePago == null) return NotFound();
            
            // Devolver DTO para evitar referencias circulares en serialización JSON
            var dto = new
            {
                stripePago.Id,
                stripePago.UsuarioId,
                stripePago.PedidoId,
                stripePago.StripeSessionId,
                stripePago.Monto,
                stripePago.TipoOperacion,
                stripePago.Estado
            };
            return Ok(dto);
        }

        [Authorize]
        [HttpPut("stripe/estado/{sessionId}")]
        public async Task<ActionResult> ActualizarEstadoStripe(string sessionId, [FromBody] ActualizarEstadoStripeRequestDto request)
        {
            await _repository.ActualizarEstadoStripeAsync(sessionId, request.NuevoEstado);
            return Ok();
        }
    }
}