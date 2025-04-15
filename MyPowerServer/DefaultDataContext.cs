using SnapObjects.Data;
using SnapObjects.Data.SqlServer;

namespace MyPowerServer
{
    public class DefaultDataContext : SqlServerDataContext
    {
        public DefaultDataContext(string connectionString)
            : this(new SqlServerDataContextOptions<DefaultDataContext>(connectionString))
        {
        }
        
        public DefaultDataContext(IDataContextOptions<DefaultDataContext> options)
            : base(options)
        {
            // Establecer las propiedades directamente en las opciones
            if (options is SqlServerDataContextOptions sqlOptions)
            {
                sqlOptions.TrimSpaces = true;
                sqlOptions.DelimitIdentifier = false;
            }
        }
        
        public DefaultDataContext(IDataContextOptions options)
            : base(options)
        {
            // Establecer las propiedades directamente en las opciones
            if (options is SqlServerDataContextOptions sqlOptions)
            {
                sqlOptions.TrimSpaces = true;
                sqlOptions.DelimitIdentifier = false;
            }
        }
        
        // Aquí puedes añadir propiedades DbSet<T> para tus entidades si las necesitas
        // Ejemplo:
        // public DbSet<Usuario> Usuarios { get; set; }
        // public DbSet<Producto> Productos { get; set; }
    }
}