using Microsoft.EntityFrameworkCore;
using RuletaApi.Models;

namespace RuletaApi.Context
{
    public class AppDBContext: DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options): base(options)
        { 
        }
        public DbSet<Ruleta> Ruletas { get; set; }
        public DbSet<Apuesta>  Apuestas { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
    }
}
