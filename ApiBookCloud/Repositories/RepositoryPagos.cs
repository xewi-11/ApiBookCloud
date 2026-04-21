using ApiBookCloud.Data;
using Microsoft.EntityFrameworkCore;
using NugetModelsBookCloud.Models;

namespace ApiBookCloud.Repositories
{
    public class RepositoryPagos
    {
        private readonly BookCloudContext _context;

        public RepositoryPagos(BookCloudContext context)
        {
            _context = context;
        }

        #region Metodos de Pagos Tradicionales
        public async Task<int> CrearPagoAsync(Pago pago)
        {
            await _context.Pagos.AddAsync(pago);
            await _context.SaveChangesAsync();
            return pago.Id;
        }

        public async Task<Pago> GetPagoAsync(int pagoId)
        {
            return await _context.Pagos
                .Include(p => p.Pedido)
                .FirstOrDefaultAsync(p => p.Id == pagoId && p.Activo);
        }

        public async Task<List<Pago>> GetPagosPorPedidoAsync(int pedidoId)
        {
            return await _context.Pagos
                .Where(p => p.PedidoId == pedidoId && p.Activo)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();
        }

        public async Task<List<Pago>> GetPagosPorUsuarioAsync(int usuarioId)
        {
            return await _context.Pagos
                .Include(p => p.Pedido)
                .Where(p => p.Pedido.UsuarioId == usuarioId && p.Activo)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();
        }
        #endregion

        #region Metodos de Integracion Stripe
        public async Task InsertarStripePagoAsync(StripePago stripePago)
        {
            await _context.StripePagos.AddAsync(stripePago);
            await _context.SaveChangesAsync();
        }

        public async Task<StripePago> GetStripePagoBySessionIdAsync(string sessionId)
        {
            return await _context.StripePagos
                .FirstOrDefaultAsync(s => s.StripeSessionId == sessionId);
        }

        public async Task ActualizarEstadoStripeAsync(string sessionId, string nuevoEstado)
        {
            var registro = await GetStripePagoBySessionIdAsync(sessionId);

            if (registro == null)
            {
                return;
            }

            if (registro.Estado != nuevoEstado)
            {
                registro.Estado = nuevoEstado;
            }

            if (nuevoEstado == "completed")
            {
                if (registro.TipoOperacion == "COMPRA" && registro.PedidoId.HasValue)
                {
                    int pedidoId = registro.PedidoId.Value;

                    bool existePagoCompletado = await _context.Pagos.AnyAsync(p =>
                        p.PedidoId == pedidoId
                        && p.Estado == "Completado"
                        && p.Activo);

                    if (!existePagoCompletado)
                    {
                        var pagoOficial = new Pago
                        {
                            PedidoId = pedidoId,
                            Monto = registro.Monto,
                            FechaPago = DateTime.Now,
                            Metodo = "Stripe",
                            Estado = "Completado",
                            Activo = true
                        };
                        await _context.Pagos.AddAsync(pagoOficial);
                    }

                    var pedido = await _context.Pedidos
                        .Include(p => p.PedidoDetalles)
                            .ThenInclude(d => d.Libro)
                        .FirstOrDefaultAsync(p => p.Id == pedidoId);

                    if (pedido != null)
                    {
                        var ventasPorVendedor = pedido.PedidoDetalles
                            .Where(d => d.Activo && d.Libro != null)
                            .GroupBy(d => d.Libro.UsuarioId)
                            .Select(g => new
                            {
                                VendedorId = g.Key,
                                TotalVenta = g.Sum(d => d.PrecioUnitario * d.Cantidad)
                            })
                            .ToList();

                        foreach (var venta in ventasPorVendedor)
                        {
                            bool existeMovimientoIngreso = await _context.SaldoMovimientos.AnyAsync(m =>
                                m.UsuarioId == venta.VendedorId
                                && m.PedidoId == pedidoId
                                && m.Tipo == "Ingreso"
                                && m.Activo);

                            if (!existeMovimientoIngreso)
                            {
                                var movimiento = new SaldoMovimiento
                                {
                                    UsuarioId = venta.VendedorId,
                                    PedidoId = pedidoId,
                                    Monto = venta.TotalVenta,
                                    Tipo = "Ingreso",
                                    Descripcion = $"Venta de libro(s) - Pedido #{pedidoId}",
                                    Fecha = DateTime.Now,
                                    Activo = true
                                };

                                await _context.SaldoMovimientos.AddAsync(movimiento);
                            }
                        }

                        pedido.Estado = "Completado";
                    }
                }

                if (registro.TipoOperacion == "RECARGA")
                {
                    var movimiento = new SaldoMovimiento
                    {
                        UsuarioId = registro.UsuarioId,
                        Monto = registro.Monto,
                        Tipo = "Ingreso",
                        Descripcion = "Recarga de saldo via Stripe (Tarjeta)",
                        Fecha = DateTime.Now,
                        Activo = true
                    };
                    await _context.SaldoMovimientos.AddAsync(movimiento);
                }
            }

            await _context.SaveChangesAsync();
        }
        #endregion
    }
}