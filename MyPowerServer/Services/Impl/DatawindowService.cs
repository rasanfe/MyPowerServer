using SnapObjects.Data;
using DWNet.Data;
using PowerScript.Bridge;


namespace MyPowerServer.Services.Impl
{
    public class DatawindowService : IDatawindowService
    {
        
        private readonly DataContext _dataContext;
        private readonly IConfiguration _configuration;
             
        public DatawindowService(DefaultDataContext dataContext, IConfiguration configuration)
        {
            _dataContext = dataContext;
            _configuration = configuration;
        }
        
        public async Task<object> RetrieveAsync(string dwSyntaxEncoded, object[] parametersSelect, CancellationToken cancellationToken)
        {
            //1- Decodificamos Sintaxis
            String dwSyntax = Decode(dwSyntaxEncoded);
            
            //2- Eliminamos los Campos Computados de la sintaxi
            dwSyntax = RemoveComputeLines(dwSyntax);
            
            //3- Ceamos DataObject a partir de la sintaxis (SRD)
            var dataObject = new DataObject(dwSyntax, _dataContext);
            
            //4- Creamos DataStore
            var dataStore = DataStore.Create(dataObject, _dataContext);
            
            //5- Hacemos el Retirve y devolvemos el Datastore
            var rowCount = await dataStore.RetrieveAsync(parametersSelect, cancellationToken);
            return dataStore;           
        }
        
       
        public async Task<int> UpdateAsync(string dwSyntaxEncoded, object[] jsonUpdate, CancellationToken cancellationToken)
        {
            //1- Decodificamos Sintaxis
            String dwSyntax = Decode(dwSyntaxEncoded);

            //2- Eliminamos los Campos Computados de la sintaxi
            dwSyntax = RemoveComputeLines(dwSyntax);

            //3- Ceamos DataObject a partir de la sintaxis (SRD)
            var dataObject = new DataObject(dwSyntax, _dataContext);

            //4- Creamos DataStore
            var dataStore = DataStore.Create(dataObject, _dataContext);

            //5- Importamos el Json Recibido con los bufers al nuevo datastore
            string jsonStringEncoded = jsonUpdate[0]?.ToString();
            string jsonString = Decode(jsonStringEncoded);
            var rowCount = dataStore.ImportJson(jsonString);
       
            //6- Hacemos el Update
            int result =  await dataStore.UpdateAsync(cancellationToken);

            return result;
        }

        public async Task<object> CargarAsync(string sqlEnncoded, object[] parametersSelect, CancellationToken cancellationToken) 
        {
            // 1- Decodificamos SQl
            string sqlDecoded = Decode(sqlEnncoded);
            
            //2-Hacemos el Select con un Modelo Dinámico
            var result = await _dataContext.SqlExecutor.SelectAsync<DynamicModel>(sqlDecoded, parametersSelect, cancellationToken);

            return result;
        }
        
        private string RemoveComputeLines(string dwSyntax)
        {
            if (string.IsNullOrEmpty(dwSyntax))
                return dwSyntax;

            // Dividir el texto en líneas
            var lines = dwSyntax.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // Filtrar las líneas que no empiezan con "compute" (ignorando espacios y mayúsculas/minúsculas)
            var filteredLines = lines.Where(line =>
                !line.TrimStart().StartsWith("compute", StringComparison.OrdinalIgnoreCase));

            // Volver a unir las líneas
            return string.Join(Environment.NewLine, filteredLines);
        }

        private string Decode(string sqlEncoded)
        {
            string sqlDecoded = CoderClass.Base64UrlDecode(sqlEncoded);

            return sqlDecoded;
        }

    }
}
