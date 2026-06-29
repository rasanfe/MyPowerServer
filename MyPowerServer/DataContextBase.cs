using SnapObjects.Data;
using SnapObjects.Data.SqlServer;

namespace MyPowerServer
{
    /// <summary>
    /// Contexto de datos "base" sobre SQL Server. Es una plantilla equivalente a
    /// <see cref="DefaultDataContext"/> pero sin los ajustes (TrimSpaces, etc.). Se deja como
    /// punto de partida por si necesitáis un segundo contexto con otra configuración; el
    /// servidor usa por defecto <see cref="DefaultDataContext"/>.
    /// </summary>
    public class DataContextBase : SqlServerDataContext
    {
        public DataContextBase(string connectionString)
            : this(new SqlServerDataContextOptions<DataContextBase>(connectionString))
        {
        }

        public DataContextBase(IDataContextOptions<DataContextBase> options)
            : base(options)
        {
        }

        public DataContextBase(IDataContextOptions options)
            : base(options)
        {
        }
    }
}
