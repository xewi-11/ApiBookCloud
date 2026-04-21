using ApiBookCloud.Data;
using Microsoft.EntityFrameworkCore;
using NugetModelsBookCloud.Models; // Usar el NuGet de modelos

namespace ApiBookCloud.Repositories
{
    public class RepositoryFavoritos
    {
        private BookCloudContext _context;

        public RepositoryFavoritos(BookCloudContext context)
        {
            this._context = context;
        }

        public async Task<List<Favorito>> GetFavoritosByUsuarioIdAsync(int usuarioId)
        {
            return await _context.Favoritos
                .Include(f => f.Libro)
                .Where(f => f.UsuarioId == usuarioId && f.Activo)
                .ToListAsync();
        }

        public async Task<Favorito> GetFavoritoByUsuarioAndLibroAsync(int usuarioId, int libroId)
        {
            return await _context.Favoritos
                .FirstOrDefaultAsync(f => f.UsuarioId == usuarioId && f.LibroId == libroId && f.Activo);
        }

        public async Task InsertFavoritoAsync(Favorito favorito)
        {
            await _context.Favoritos.AddAsync(favorito);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFavoritoAsync(int id)
        {
            var favorito = await _context.Favoritos.FindAsync(id);
            if (favorito != null)
            {
                favorito.Activo = false;
                _context.Favoritos.Update(favorito);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Libro>> GetFavoritosByUsuarioAsync(int usuarioId)
        {
            return await _context.Favoritos
                .Include(f => f.Libro)
                .Where(f => f.UsuarioId == usuarioId && f.Activo)
                .Select(f => f.Libro)
                .ToListAsync();
        }

        public async Task AddFavoritoAsync(int usuarioId, int libroId)
        {
            // Verificar si ya existe
            var existente = await GetFavoritoByUsuarioAndLibroAsync(usuarioId, libroId);

            if (existente != null)
            {
                // Si existe pero está inactivo, reactivarlo
                if (!existente.Activo)
                {
                    existente.Activo = true;
                    existente.FechaAgregado = DateTime.Now;
                    _context.Favoritos.Update(existente);
                    await _context.SaveChangesAsync();
                }
                return;
            }

            // Si no existe, crear uno nuevo
            var favorito = new Favorito
            {
                UsuarioId = usuarioId,
                LibroId = libroId,
                FechaAgregado = DateTime.Now,
                Activo = true
            };

            await InsertFavoritoAsync(favorito);
        }

        public async Task RemoveFavoritoAsync(int usuarioId, int libroId)
        {
            var favorito = await GetFavoritoByUsuarioAndLibroAsync(usuarioId, libroId);

            if (favorito != null)
            {
                await DeleteFavoritoAsync(favorito.Id);
            }
        }
    }
}