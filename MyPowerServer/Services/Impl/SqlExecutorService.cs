using SnapObjects.Data;

namespace MyPowerServer.Services.Impl
{
    /// <summary>
    /// Implementación de <see cref="ISqlExecutorService"/>: ejecuta SQL directo (INSERT/UPDATE/
    /// DELETE/SELECT) contra la BD usando el <c>SqlExecutor</c> del contexto de Appeon. El SQL
    /// llega codificado en Base64Url; aquí se decodifica y se ejecuta con sus parámetros.
    /// </summary>
    public class SqlExecutorService : ISqlExecutorService
    {
        private readonly DataContext _dataContext;       // Conexión a la BD (elegida por la fábrica).
        private readonly IConfiguration _configuration;  // Reservado para futuras opciones de appsettings.

        public SqlExecutorService(DefaultDataContext dataContext, IConfiguration configuration)
        {
            _dataContext = dataContext;
            _configuration = configuration;
        }

        /// <inheritdoc/>
        public async Task<int> InsertAsync(string sqlEncoded, object[] parametersInsert, CancellationToken cancellationToken)
        {
            string sqlDecoded = Decode(sqlEncoded);

            // ExecuteAsync ejecuta el comando y devuelve las filas afectadas.
            var rowsAffected = await _dataContext.SqlExecutor.ExecuteAsync(sqlDecoded, parametersInsert, cancellationToken);

            return rowsAffected;
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(string sqlEncoded, object[] parametersUpdate, CancellationToken cancellationToken)
        {
            string sqlDecoded = Decode(sqlEncoded);

            // Aquí no devolvemos el conteo (la firma es Task): sólo ejecutamos.
            var rowsAffected = await _dataContext.SqlExecutor.ExecuteAsync(sqlDecoded, parametersUpdate, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string sqlEncoded, object[] parametersDelete, CancellationToken cancellationToken)
        {
            string sqlDecoded = Decode(sqlEncoded);

            var rowsAffected = await _dataContext.SqlExecutor.ExecuteAsync(sqlDecoded, parametersDelete, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IList<DynamicModel>> SelectIntoAsync(string sqlEncoded, object[] parametersSelect, CancellationToken cancellationToken)
        {
            //1- Decodificamos el SQL.
            string sqlDecoded = Decode(sqlEncoded);

            //2- Hacemos el SELECT con Modelo Dinámico (sin clase por tabla).
            var list = await _dataContext.SqlExecutor.SelectAsync<DynamicModel>(sqlDecoded, parametersSelect, cancellationToken);

            //3- Imitamos el SELECT...INTO de PowerScript: como mucho una fila. Si hay más,
            //   es un error de la consulta y avisamos.
            int rowCount = list.Count;

            if (rowCount > 1)
            {
                throw new Exception("La consulta SQL ha devuelto más de un resultado.");
            };

            return list;
        }

        // Atajo a la decodificación Base64Url compartida (CoderClass).
        private string Decode(string sqlEncoded)
        {
            string sqlDecoded = CoderClass.Base64UrlDecode(sqlEncoded);

            return sqlDecoded;
        }
    }
}
