
using Microsoft.EntityFrameworkCore;
using NugetModelsBookCloud.Models;

namespace ApiBookCloud.Data
{
    public class BookCloudContext : DbContext
    {
        public BookCloudContext(DbContextOptions<BookCloudContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<UsuarioSeguridad> UsuarioCredenciales { get; set; }
        public DbSet<Libro> Libros { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<SaldoMovimiento> SaldoMovimientos { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoDetalle> PedidoDetalles { get; set; }
        public DbSet<Favorito> Favoritos { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Mensaje> Mensajes { get; set; }
        public DbSet<StripePago> StripePagos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar relación uno a uno entre Usuario y UsuarioSeguridad
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.UsuarioSeguridad)
                .WithOne(us => us.Usuario)
                .HasForeignKey<UsuarioSeguridad>(us => us.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurar índices para Chats (aprovechando los que ya existen en BD)
            modelBuilder.Entity<Chat>()
                .HasIndex(c => c.Usuario1Id)
                .HasDatabaseName("IX_Chats_Usuario1Id");

            modelBuilder.Entity<Chat>()
                .HasIndex(c => c.Usuario2Id)
                .HasDatabaseName("IX_Chats_Usuario2Id");

            modelBuilder.Entity<Chat>()
                .HasIndex(c => c.Activo)
                .HasDatabaseName("IX_Chats_Activo")
                .HasFilter("[Activo] = 1");

            // Configurar índices para Mensajes
            modelBuilder.Entity<Mensaje>()
                .HasIndex(m => m.ChatId)
                .HasDatabaseName("IX_Mensajes_ChatId");

            modelBuilder.Entity<Mensaje>()
                .HasIndex(m => m.RemitenteId)
                .HasDatabaseName("IX_Mensajes_RemitenteId");

            modelBuilder.Entity<Mensaje>()
                .HasIndex(m => m.Activo)
                .HasDatabaseName("IX_Mensajes_Activo")
                .HasFilter("[Activo] = 1");
        }
    }
}
