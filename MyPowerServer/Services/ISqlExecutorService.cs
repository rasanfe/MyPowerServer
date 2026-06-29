using SnapObjects.Data;

namespace MyPowerServer.Services
{
    /// <summary>
    /// Contrato del ejecutor de SQL directo. Cada método recibe el SQL en Base64Url y el array
    /// de parámetros, y delega en el <c>SqlExecutor</c> del contexto de datos.
    /// </summary>
    public interface ISqlExecutorService
    {
        /// <summary>Ejecuta un INSERT y devuelve el número de filas afectadas.</summary>
        Task<int> InsertAsync(string sqlEncrypted, object[] parametersInsert, CancellationToken cancellationToken);

        /// <summary>Ejecuta un UPDATE.</summary>
        Task UpdateAsync(string sqlEncrypted, object[] parametersUpdate, CancellationToken cancellationToken);

        /// <summary>Ejecuta un DELETE.</summary>
        Task DeleteAsync(string sqlEncrypted, object[] parametersDelete, CancellationToken cancellationToken);

        /// <summary>SELECT que devuelve como mucho una fila (estilo SELECT...INTO de PowerScript).</summary>
        Task<IList<DynamicModel>> SelectIntoAsync(string sqlEncrypted, object[] parametersSelect, CancellationToken cancellationToken);
    }
}
