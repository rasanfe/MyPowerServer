using SnapObjects.Data;
using DWNet.Data;
using PowerScript.Bridge; // Aporta el tipo DataObject (el "puente" PowerScript ↔ .NET).

namespace MyPowerServer.Services.Impl
{
    /// <summary>
    /// Corazón del "Power Server": a partir de la SINTAXIS de una DataWindow (lo que en
    /// PowerBuilder es el .SRD) construye en memoria un <c>DataObject</c> + <c>DataStore</c>
    /// con los SDK de Appeon y ejecuta el Retrieve/Update contra SQL Server. Es, a mano, lo
    /// que PowerServer hace por debajo.
    /// </summary>
    public class DatawindowService : IDatawindowService
    {
        private readonly DataContext _dataContext;       // Conexión a la BD (la elige la fábrica por petición).
        private readonly IConfiguration _configuration;  // Acceso a appsettings; reservado para futuras opciones.

        // El framework inyecta un DefaultDataContext (resuelto por DataContextFactory según
        // la cabecera 'profile') y la configuración. Lo guardamos como DataContext (la clase base).
        public DatawindowService(DefaultDataContext dataContext, IConfiguration configuration)
        {
            _dataContext = dataContext;
            _configuration = configuration;
        }

        /// <inheritdoc/>
        public async Task<object> RetrieveAsync(string dwSyntaxEncoded, object[] parametersSelect, CancellationToken cancellationToken)
        {
            //1- Decodificamos la sintaxis (viene en Base64Url).
            String dwSyntax = Decode(dwSyntaxEncoded);

            //2- Quitamos los campos computados de la sintaxis: no existen en la BD, sólo
            //   confundirían al motor al montar el SELECT.
            dwSyntax = RemoveComputeLines(dwSyntax);

            //3- Creamos el DataObject a partir de la sintaxis (el equivalente al SRD de PB).
            var dataObject = new DataObject(dwSyntax, _dataContext);

            //4- Creamos el DataStore (el "DataStore" de toda la vida, pero en .NET).
            var dataStore = DataStore.Create(dataObject, _dataContext);

            //5- Hacemos el Retrieve con sus parámetros y devolvemos el DataStore ya relleno.
            //   El SDK se encarga luego de serializarlo a JSON para el cliente.
            var rowCount = await dataStore.RetrieveAsync(parametersSelect, cancellationToken);
            return dataStore;
        }

        /// <inheritdoc/>
        public async Task<int> UpdateAsync(string dwSyntaxEncoded, object[] jsonUpdate, CancellationToken cancellationToken)
        {
            //1- Decodificamos la sintaxis.
            String dwSyntax = Decode(dwSyntaxEncoded);

            //2- Quitamos los campos computados.
            dwSyntax = RemoveComputeLines(dwSyntax);

            //3- Creamos el DataObject a partir de la sintaxis (SRD).
            var dataObject = new DataObject(dwSyntax, _dataContext);

            //4- Creamos el DataStore.
            var dataStore = DataStore.Create(dataObject, _dataContext);

            //5- Importamos al DataStore el JSON con los buffers que mandó el cliente. Aquí está
            //   la magia: ImportJson reconstruye los cambios (altas/bajas/modificaciones) tal y
            //   como los tenía la DataWindow del cliente, incluido el estado de cada fila.
            string jsonStringEncoded = jsonUpdate[0]!.ToString()!;
            string jsonString = Decode(jsonStringEncoded);
            var rowCount = dataStore.ImportJson(jsonString);

            //6- Hacemos el Update: el DataStore genera y ejecuta los INSERT/UPDATE/DELETE necesarios.
            int result = await dataStore.UpdateAsync(cancellationToken);

            return result;
        }

        /// <inheritdoc/>
        public async Task<object> CargarAsync(string sqlEnncoded, object[] parametersSelect, CancellationToken cancellationToken)
        {
            //1- Decodificamos el SQL.
            string sqlDecoded = Decode(sqlEnncoded);

            //2- Hacemos el SELECT con un Modelo Dinámico (DynamicModel): no hace falta una clase
            //   por tabla; el SDK mapea las columnas que vengan. Cómodo para consultas ad-hoc.
            var result = await _dataContext.SqlExecutor.SelectAsync<DynamicModel>(sqlDecoded, parametersSelect, cancellationToken);

            return result;
        }

        // Elimina de la sintaxis las líneas de campos computados ("compute ...").
        private string RemoveComputeLines(string dwSyntax)
        {
            if (string.IsNullOrEmpty(dwSyntax))
                return dwSyntax;

            // Partimos por saltos de línea (cubrimos \r\n, \r y \n).
            var lines = dwSyntax.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // Nos quedamos con las líneas que NO empiezan por "compute" (ignorando espacios y
            // mayúsculas/minúsculas).
            var filteredLines = lines.Where(line =>
                !line.TrimStart().StartsWith("compute", StringComparison.OrdinalIgnoreCase));

            // Reunimos las líneas que sobreviven.
            return string.Join(Environment.NewLine, filteredLines);
        }

        // Atajo a la decodificación Base64Url compartida (CoderClass).
        private string Decode(string sqlEncoded)
        {
            string sqlDecoded = CoderClass.Base64UrlDecode(sqlEncoded);

            return sqlDecoded;
        }
    }
}
