using ApiBookCloud.Data;
using Microsoft.EntityFrameworkCore;
using NugetModelsBookCloud.Models; // Usar el NuGet de modelos

namespace ApiBookCloud.Repositories
{
    public class RepositoryPedidos
    {
        private readonly BookCloudContext _context;

        public RepositoryPedidos(BookCloudContext context)
        {
            _context = context;
        }

        public async Task<int> CrearPedidoAsync(int usuarioId, decimal total, List<PedidoDetalle> detalles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Crear el pedido
                var pedido = new Pedido
                {
                    UsuarioId = usuarioId,
                    FechaPedido = DateTime.Now,
                    Total = total,
                    Estado = "Pendiente",
                    Activo = true
                };

                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();

                // Agregar los detalles
                foreach (var detalle in detalles)
                {
                    detalle.PedidoId = pedido.Id;
                    _context.PedidoDetalles.Add(detalle);

                    // Descontar stock
                    var libro = await _context.Libros.FindAsync(detalle.LibroId);
                    if (libro != null)
                    {
                        libro.Stock -= detalle.Cantidad;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return pedido.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Pedido> GetPedidoAsync(int pedidoId)
        {
            return await _context.Pedidos
                .Include(p => p.PedidoDetalles)
                .ThenInclude(pd => pd.Libro)
                .FirstOrDefaultAsync(p => p.Id == pedidoId);
        }

        public async Task<List<Pedido>> GetPedidosUsuarioAsync(int usuarioId)
        {
            return await _context.Pedidos
                .Include(p => p.PedidoDetalles)
                    .ThenInclude(pd => pd.Libro)
                .Include(p => p.Pagos)
                .Where(p => p.UsuarioId == usuarioId && p.Activo)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();
        }

        public async Task ActualizarEstadoPedidoAsync(int pedidoId, string estado)
        {
            var pedido = await _context.Pedidos.FindAsync(pedidoId);
            if (pedido != null)
            {
                pedido.Estado = estado;
                await _context.SaveChangesAsync();
            }
        }
    }
}