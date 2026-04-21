using ApiBookCloud.Data;
using Microsoft.EntityFrameworkCore;
using NugetModelsBookCloud.Models;

namespace ApiBookCloud.Repositories
{
    public class RepositoryWallet
    {
        private readonly BookCloudContext _context;

        public RepositoryWallet(BookCloudContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetSaldoUsuarioAsync(int usuarioId)
        {
            var saldo = await _context.SaldoMovimientos
                .Where(m => m.UsuarioId == usuarioId && m.Activo)
                .SumAsync(m => m.Tipo == "Ingreso" ? m.Monto : -m.Monto);

            return saldo;
        }

        public async Task<List<SaldoMovimiento>> GetMovimientosAsync(int usuarioId, int limit = 20)
        {
            return await _context.SaldoMovimientos
                .Where(m => m.UsuarioId == usuarioId && m.Activo)
                .OrderByDescending(m => m.Fecha)
                .Take(limit)
                .Include(m => m.Pedido)
                .ToListAsync();
        }

        public async Task RecargarSaldoAsync(int usuarioId, decimal monto, string descripcion)
        {
            if (monto <= 0)
                throw new ArgumentException("El monto debe ser mayor a 0");

            var movimiento = new SaldoMovimiento
            {
                UsuarioId = usuarioId,
                Monto = monto,
                Tipo = "Ingreso",
                Descripcion = descripcion ?? "Recarga de saldo",
                Fecha = DateTime.Now,
                Activo = true
            };

            await _context.SaldoMovimientos.AddAsync(movimiento);
            await _context.SaveChangesAsync();
        }

        public async Task DescontarSaldoAsync(int usuarioId, int pedidoId, decimal monto, string descripcion)
        {
            if (monto <= 0)
                throw new ArgumentException("El monto debe ser mayor a 0");

            var saldoActual = await GetSaldoUsuarioAsync(usuarioId);
            if (saldoActual < monto)
                throw new InvalidOperationException("Saldo insuficiente");

            var movimiento = new SaldoMovimiento
            {
                UsuarioId = usuarioId,
                PedidoId = pedidoId,
                Monto = monto,
                Tipo = "Pago",
                Descripcion = descripcion ?? $"Pago de pedido #{pedidoId}",
                Fecha = DateTime.Now,
                Activo = true
            };

            await _context.SaldoMovimientos.AddAsync(movimiento);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> TieneSaldoSuficienteAsync(int usuarioId, decimal monto)
        {
            var saldo = await GetSaldoUsuarioAsync(usuarioId);
            return saldo >= monto;
        }

        public async Task TransferirSaldoAVendedoresAsync(int pedidoId, int compradorId)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.PedidoDetalles)
                    .ThenInclude(d => d.Libro)
                .FirstOrDefaultAsync(p => p.Id == pedidoId);

            if (pedido == null)
                throw new InvalidOperationException($"No se encontró el pedido #{pedidoId}");

            var ventasPorVendedor = pedido.PedidoDetalles
                .Where(d => d.Activo)
                .GroupBy(d => d.Libro.UsuarioId)
                .Select(g => new
                {
                    VendedorId = g.Key,
                    TotalVenta = g.Sum(d => d.PrecioUnitario * d.Cantidad)
                })
                .ToList();

            foreach (var venta in ventasPorVendedor)
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

            await _context.SaveChangesAsync();
        }
    }
}