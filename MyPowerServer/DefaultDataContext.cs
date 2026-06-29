using SnapObjects.Data;
using SnapObjects.Data.SqlServer;

namespace MyPowerServer
{
    /// <summary>
    /// Contexto de datos de la aplicación contra SQL Server. Hereda de
    /// <see cref="SqlServerDataContext"/> (de los SDK de Appeon) y es el equivalente al
    /// "Transaction" (SQLCA) de PowerBuilder: representa la conexión y por él pasan los
    /// Retrieve/Update. La fábrica <see cref="DataContextFactory"/> crea uno por petición
    /// con la cadena de conexión del perfil pedido.
    /// </summary>
    public class DefaultDataContext : SqlServerDataContext
    {
        // Atajo: construir el contexto directamente a partir de una cadena de conexión.
        public DefaultDataContext(string connectionString)
            : this(new SqlServerDataContextOptions<DefaultDataContext>(connectionString))
        {
        }

        public DefaultDataContext(IDataContextOptions<DefaultDataContext> options)
            : base(options)
        {
            // Ajustamos el comportamiento del contexto sobre las propias opciones:
            ConfigureOptions(options);
        }

        public DefaultDataContext(IDataContextOptions options)
            : base(options)
        {
            ConfigureOptions(options);
        }

        // TrimSpaces: recorta los espacios de relleno típicos de columnas CHAR (como hace PB).
        // DelimitIdentifier=false: no envuelve los identificadores entre corchetes [].
        private static void ConfigureOptions(IDataContextOptions options)
        {
            if (options is SqlServerDataContextOptions sqlOptions)
            {
                sqlOptions.TrimSpaces = true;
                sqlOptions.DelimitIdentifier = false;
            }
        }

        // Aquí podrías añadir propiedades DbSet<T> para tus entidades si las necesitas.
        // Ejemplo:
        // public DbSet<Usuario> Usuarios { get; set; }
        // public DbSet<Producto> Productos { get; set; }
    }
}
