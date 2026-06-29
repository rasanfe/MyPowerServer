using SnapObjects.Data;
using DWNet.Data;

namespace MyPowerServer.Services
{
    /// <summary>
    /// Contrato del servicio que opera con DataWindows. Todas las entradas de texto
    /// (sintaxis y SQL) llegan codificadas en Base64Url; la implementación las decodifica.
    /// Devuelven <c>object</c> porque lo que viaja es un DataStore que serializan los SDK de Appeon.
    /// </summary>
    public interface IDatawindowService
    {
        /// <summary>Ejecuta el Retrieve de la DataWindow y devuelve el DataStore relleno.</summary>
        Task<object> RetrieveAsync(string dwSyntaxEncoded, object[] parametersSelect, CancellationToken cancellationToken);

        /// <summary>Importa los buffers (JSON) y persiste los cambios; devuelve filas afectadas.</summary>
        Task<int> UpdateAsync(string dwSyntaxEncoded, object[] jsonUpdate, CancellationToken cancellationToken);

        /// <summary>SELECT libre con modelo dinámico (sin DataWindow).</summary>
        Task<object> CargarAsync(string sqlEncoded, object[] parametersSelect, CancellationToken cancellationToken);
    }
}
