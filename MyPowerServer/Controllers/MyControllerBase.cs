using Microsoft.AspNetCore.Mvc;

namespace MyPowerServer.Controllers
{
    /// <summary>
    /// Clase base común de los controladores de este servidor. Hereda de <see cref="ControllerBase"/>
    /// (controlador de API sin vistas) y añade el helper compartido para descomponer los parámetros
    /// que llegan en el cuerpo de la petición. Así no repetimos esa lógica en cada controlador.
    /// </summary>
    public class MyControllerBase : ControllerBase
    {
        /// <summary>
        /// Separa el diccionario recibido en (a) la sentencia SQL/sintaxis codificada (el primer valor)
        /// y (b) el array de parámetros (el resto). Es un simple envoltorio sobre
        /// <see cref="JsonProcessorHelper.GetQueryParams"/> para tenerlo a mano en los controladores.
        /// </summary>
        protected void GetQueryParams(Dictionary<string, object> queryParams, ref string sqlEncoded, ref object[] parameters)
        {
            JsonProcessorHelper.GetQueryParams(queryParams, ref sqlEncoded, ref parameters);
        }
    }
}
