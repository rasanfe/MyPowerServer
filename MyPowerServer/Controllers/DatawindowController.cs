using Microsoft.AspNetCore.Mvc;
using DWNet.Data;
using MyPowerServer.Services;
using SnapObjects.Data;

namespace MyPowerServer.Controllers
{
    /// <summary>
    /// Endpoints "estilo DataWindow": reciben la sintaxis de una DataWindow (codificada en
    /// Base64Url) más sus parámetros y delegan en <see cref="IDatawindowService"/>. Ruta
    /// <c>api/Datawindow/[action]</c>, o sea: <c>/Retrieve</c>, <c>/Update</c> y <c>/Cargar</c>.
    /// <para>
    /// Todos los endpoints reciben el MISMO formato: un diccionario donde el primer valor es
    /// la sintaxis/SQL codificada y el resto son los parámetros del SELECT/UPDATE.
    /// <see cref="MyControllerBase.GetQueryParams"/> se encarga de partirlo.
    /// </para>
    /// </summary>
    [Route("api/[controller]/[action]")]
	[ApiController]
    // Constructor primario: el framework inyecta el servicio de DataWindow.
	public class DatawindowController(IDatawindowService idatawindowService) : MyControllerBase
    {
        /// <summary>
        /// <c>POST api/Datawindow/Retrieve</c> — ejecuta el Retrieve de la DataWindow y
        /// devuelve el DataStore resultante (serializado a JSON por los SDK de Appeon).
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> RetrieveAsync([FromBody] Dictionary<string, object> queryParams)
        {
            //1- Procesar Parámetros Recibidos: separar sintaxis (1er valor) de los parámetros.
            string dwSyntaxEncoded = string.Empty;
            object[]? parametersSelect = new object[queryParams.Count - 1];
            GetQueryParams(queryParams, ref dwSyntaxEncoded, ref parametersSelect);
            try
            {
                //2- Llamar al servicio con la sintaxis en Base64Url y los parámetros.
                var result = await idatawindowService.RetrieveAsync(dwSyntaxEncoded, parametersSelect, default);
                return Ok(result);
            }
            catch (Exception ex)
            {
                //3- Control de Errores: cualquier fallo se devuelve como 500 con el mensaje.
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// <c>POST api/Datawindow/Update</c> — recibe los buffers de la DataWindow en JSON,
        /// monta el DataStore y persiste los cambios en SQL Server. Devuelve el nº de filas afectadas.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> UpdateAsync([FromBody] Dictionary<string, object> queryParams)
        {
            //1- Procesar Parámetros Recibidos
            string dwSyntaxEncoded = string.Empty;
            object[]? parametersUpdate = new object[queryParams.Count - 1];
            GetQueryParams(queryParams, ref dwSyntaxEncoded, ref parametersUpdate);
            try
            {
                //2- Llamar al servicio con la sintaxis en Base64Url y los parámetros.
                int result = await idatawindowService.UpdateAsync(dwSyntaxEncoded, parametersUpdate, default);
                return Ok(result);
            }
            catch (Exception ex)
            {
                //3- Control de Errores
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// <c>POST api/Datawindow/Cargar</c> — ejecuta un SELECT libre (sin DataWindow) usando
        /// un modelo dinámico y devuelve el resultado. Atajo cómodo para cargas rápidas.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> CargarAsync([FromBody] Dictionary<string, object> queryParams)
        {
            //1- Procesar Parámetros Recibidos
            string sqlEncoded = string.Empty;
            object[]? parametersSelect = new object[queryParams.Count - 1];
            GetQueryParams(queryParams, ref sqlEncoded, ref parametersSelect);
            try
            {
                //2- Llamar al servicio con el SQL en Base64Url y los parámetros.
                var result = await idatawindowService.CargarAsync(sqlEncoded, parametersSelect, default);
                return Ok(result);
            }
            catch (Exception ex)
            {
                //3- Control de Errores
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
