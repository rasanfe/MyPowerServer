using SnapObjects.Data;
using System;
using System.Text.RegularExpressions;

namespace MyPowerServer.Services.Impl
{
    public class SqlExecutorService : ISqlExecutorService
    {
        private readonly DataContext _dataContext;
        private readonly IConfiguration _configuration;
        
        public SqlExecutorService(DefaultDataContext dataContext, IConfiguration configuration)
        {
            _dataContext = dataContext;
            _configuration = configuration;
        }
        public async Task<int> InsertAsync(string sqlEncoded, object[] parametersInsert, CancellationToken cancellationToken)
        {
            string sqlDecoded = Decode(sqlEncoded);
            
            var rowsAffected = await _dataContext.SqlExecutor.ExecuteAsync(sqlDecoded, parametersInsert, cancellationToken);
            
            return rowsAffected;
        }
        public async Task UpdateAsync(string sqlEncoded, object[] parametersUpdate, CancellationToken cancellationToken)
        {
            string sqlDecoded = Decode(sqlEncoded);
            
            var rowsAffected = await _dataContext.SqlExecutor.ExecuteAsync(sqlDecoded, parametersUpdate, cancellationToken);
        }
        
        public async Task DeleteAsync(string sqlEncoded, object[] parametersDelete, CancellationToken cancellationToken)
        {
            string sqlDecoded = Decode(sqlEncoded);
            
            var rowsAffected = await _dataContext.SqlExecutor.ExecuteAsync(sqlDecoded, parametersDelete, cancellationToken);
            
        }
        public async Task<IList<DynamicModel>> SelectIntoAsync(string sqlEncoded, object[] parametersSelect, CancellationToken cancellationToken)
        {
            //1- Decodificamos el SQL
            string sqlDecoded = Decode(sqlEncoded);
            
            //2- Hacemos Select con Modelo Dinámico
            var list = await _dataContext.SqlExecutor.SelectAsync<DynamicModel>(sqlDecoded, parametersSelect, cancellationToken);
            
            //3-Limitamos a devolver sólo una Fila
            int rowCount = list.Count;
            
            if (rowCount > 1)
            {
                throw new Exception("La consulta SQL ha devuelto más de un resultado.");
            };
            
            return list;
        }
        
        private string Decode(string sqlEncoded)
        {
            string sqlDecoded = CoderClass.Base64UrlDecode(sqlEncoded);
            
            return sqlDecoded;
        }
    }
}
