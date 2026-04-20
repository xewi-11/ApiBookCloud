
using ApiBookCloud.Data;
using Microsoft.EntityFrameworkCore;
using NugetModelsBookCloud.Models;
using NugetModelsBookCloud.Repositories.Interfaces;

namespace ApiBookCloud.Repositories
{
    public class RepositoryUsuarios : IRepositoryUsuarios
    {
        private BookCloudContext _context;

        public RepositoryUsuarios(BookCloudContext context)
        {
            this._context = context;
        }

        public async Task CreateUserASync(Usuario user, UsuarioSeguridad seguridad)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await this._context.Usuarios.AddAsync(user);
                await this._context.SaveChangesAsync();

                seguridad.UsuarioId = user.Id;
                await this._context.UsuarioCredenciales.AddAsync(seguridad);
                await this._context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Usuario> GetInfoUsario(string id)
        {
            return await _context.Usuarios
                .Include(u => u.UsuarioSeguridad)
                .FirstOrDefaultAsync(u => u.Id.ToString() == id);
        }

        public async Task<Usuario> GetUserByEmail(string email)
        {
            return await _context.Usuarios
                .Include(u => u.UsuarioSeguridad)
                .FirstOrDefaultAsync(u => u.Correo == email);
        }

        public async Task<Usuario?> GetUserById(int id)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id && u.Activo);
        }

        public async Task ActualizarUsuarioAsync(Usuario user)
        {
            _context.Usuarios.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarSeguridadUsuarioAsync(UsuarioSeguridad seguridad)
        {
            _context.UsuarioCredenciales.Update(seguridad);
            await _context.SaveChangesAsync();
        }

        public async Task<UsuarioSeguridad> GetSeguridadUsuario(int idUsuario)
        {
            return await _context.UsuarioCredenciales
                .FirstOrDefaultAsync(us => us.UsuarioId == idUsuario);
        }
    }
}
