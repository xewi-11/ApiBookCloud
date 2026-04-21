using ApiBookCloud.Data;
using Microsoft.EntityFrameworkCore;
using NugetModelsBookCloud.Models;

namespace ApiBookCloud.Repositories
{
    public class RepositoryChats
    {
        private readonly BookCloudContext _context;

        public RepositoryChats(BookCloudContext context)
        {
            _context = context;
        }

        public async Task<Chat?> ObtenerOCrearChatAsync(int usuario1Id, int usuario2Id)
        {
            var chat = await _context.Chats
                .Where(c => c.Activo &&
                    ((c.Usuario1Id == usuario1Id && c.Usuario2Id == usuario2Id) ||
                     (c.Usuario1Id == usuario2Id && c.Usuario2Id == usuario1Id)))
                .FirstOrDefaultAsync();

            if (chat == null)
            {
                chat = new Chat
                {
                    Usuario1Id = usuario1Id,
                    Usuario2Id = usuario2Id,
                    FechaCreacion = DateTime.Now,
                    Activo = true
                };

                _context.Chats.Add(chat);
                await _context.SaveChangesAsync();
            }

            return chat;
        }

        public async Task<List<Chat>> ObtenerChatsDeUsuarioAsync(int usuarioId)
        {
            return await _context.Chats
                .Include(c => c.Usuario1)
                .Include(c => c.Usuario2)
                .Where(c => c.Activo && (c.Usuario1Id == usuarioId || c.Usuario2Id == usuarioId))
                .Select(c => new Chat
                {
                    Id = c.Id,
                    Usuario1Id = c.Usuario1Id,
                    Usuario2Id = c.Usuario2Id,
                    FechaCreacion = c.FechaCreacion,
                    Activo = c.Activo,
                    Usuario1 = c.Usuario1,
                    Usuario2 = c.Usuario2,
                    Mensajes = c.Mensajes
                        .Where(m => m.Activo)
                        .OrderByDescending(m => m.FechaEnvio)
                        .Take(1)
                        .ToList()
                })
                .OrderByDescending(c => c.Mensajes.Any() ? c.Mensajes.Max(m => m.FechaEnvio) : c.FechaCreacion)
                .ToListAsync();
        }

        public async Task<Chat?> ObtenerChatPorIdAsync(int chatId)
        {
            return await _context.Chats
                .Include(c => c.Usuario1)
                .Include(c => c.Usuario2)
                .FirstOrDefaultAsync(c => c.Id == chatId && c.Activo);
        }

        public async Task<List<Mensaje>> ObtenerMensajesDelChatAsync(int chatId, int take = 50)
        {
            return await _context.Mensajes
                .Include(m => m.Remitente)
                .Where(m => m.ChatId == chatId && m.Activo)
                .OrderByDescending(m => m.FechaEnvio)
                .Take(take)
                .OrderBy(m => m.FechaEnvio)
                .ToListAsync();
        }

        public async Task<Mensaje> EnviarMensajeAsync(int chatId, int remitenteId, string contenido)
        {
            var mensaje = new Mensaje
            {
                ChatId = chatId,
                RemitenteId = remitenteId,
                Contenido = contenido,
                FechaEnvio = DateTime.Now,
                Activo = true,
                TieneUbicacion = false
            };

            _context.Mensajes.Add(mensaje);
            await _context.SaveChangesAsync();

            await _context.Entry(mensaje)
                .Reference(m => m.Remitente)
                .LoadAsync();

            return mensaje;
        }

        public async Task<Mensaje> EnviarUbicacionAsync(int chatId, int remitenteId, decimal latitud, decimal longitud)
        {
            var mensaje = new Mensaje
            {
                ChatId = chatId,
                RemitenteId = remitenteId,
                Contenido = "📍 Ubicación compartida",
                FechaEnvio = DateTime.Now,
                Activo = true,
                TieneUbicacion = true,
                Latitud = latitud,
                Longitud = longitud
            };

            _context.Mensajes.Add(mensaje);
            await _context.SaveChangesAsync();

            await _context.Entry(mensaje)
                .Reference(m => m.Remitente)
                .LoadAsync();

            return mensaje;
        }

        public async Task<bool> UsuarioPerteneceChatAsync(int chatId, int usuarioId)
        {
            return await _context.Chats
                .AnyAsync(c => c.Id == chatId &&
                    c.Activo &&
                    (c.Usuario1Id == usuarioId || c.Usuario2Id == usuarioId));
        }
    }
}