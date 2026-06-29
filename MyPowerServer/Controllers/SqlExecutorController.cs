using Microsoft.AspNetCore.Mvc;
using MyPowerServer.Services;
using DWNet.Data;
using SnapObjects.Data;

namespace MyPowerServer.Controllers
{
    /// <summary>
    /// Endpoints para ejecutar SQL "a pelo" (sin DataWindow) contra la BD, bajo la ruta
    /// <c>api/SqlExecutor/[action]</c>. Cada verbo HTTP encaja con su operación:
    /// <list type="bullet">
    ///   <item><description><c>POST   /Insert</c> — inserta y devuelve filas afectadas.</description></item>
    ///   <item><description><c>PATCH  /Update</c> — actualiza.</description></item>
    ///   <item><description><c>DELETE /Delete</c> — borra.</description></item>
    ///   <item><description><c>POST   /SelectInto</c> — SELECT que devuelve UNA fila (modelo dinámico).</description></item>
    /// </list>
    /// Mismo formato de entrada que el DatawindowController: 1er valor = SQL en Base64Url,
    /// resto = parámetros.
    /// </summary>
    [Route("api/[controller]/[action]")]
	[ApiController]
    // Constructor primario: el framework inyecta el ejecutor de SQL.
	public class SqlExecutorController(ISqlExecutorService iSqlExecutorService) : MyControllerBase
    {
        /// <summary><c>POST api/SqlExecutor/Insert</c> — ejecuta un INSERT y devuelve el nº de filas afectadas.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> InsertAsync([FromBody] Dictionary<string, object> queryParams)
        {
            //1- Procesar Parámetros Recibidos
            string sqlEncoded = string.Empty;
            object[]? parametersInsert = new object[queryParams.Count - 1];
            GetQueryParams(queryParams, ref sqlEncoded, ref parametersInsert);

            try
            {
                //2- Llamar al servicio con el SQL en Base64Url y los parámetros
                var retorno = await iSqlExecutorService.InsertAsync(sqlEncoded, parametersInsert, default);
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                //3- Control de Errores
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary><c>PATCH api/SqlExecutor/Update</c> — ejecuta un UPDATE. Devuelve <c>true</c> si fue bien.</summary>
        [HttpPatch]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> UpdateAsync([FromBody] Dictionary<string, object> queryParams)
        {
            //1- Procesar Parámetros Recibidos
            string sqlEncoded = string.Empty;
            object[]? parametersUpdate = new object[queryParams.Count - 1];
            GetQueryParams(queryParams, ref sqlEncoded, ref parametersUpdate);

            try
            {
                //2- Llamar al servicio con el SQL en Base64Url y los parámetros
                await iSqlExecutorService.UpdateAsync(sqlEncoded, parametersUpdate, default);
                return Ok(true);
            }
            catch (Exception ex)
            {
                //3- Control de Errores
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary><c>DELETE api/SqlExecutor/Delete</c> — ejecuta un DELETE. Devuelve <c>true</c> si fue bien.</summary>
        [HttpDelete]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> DeleteAsync([FromBody] Dictionary<string, object> queryParams)
        {
            //1- Procesar Parámetros Recibidos
            string sqlEncoded = string.Empty;
            object[]? parametersDelete = new object[queryParams.Count - 1];
            GetQueryParams(queryParams, ref sqlEncoded, ref parametersDelete);

            try
            {
                //2- Llamar al servicio con el SQL en Base64Url y los parámetros
                await iSqlExecutorService.DeleteAsync(sqlEncoded, parametersDelete, default);
                return Ok(true);
            }
            catch (Exception ex)
            {
                //3- Control de Errores
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// <c>POST api/SqlExecutor/SelectInto</c> — equivalente al <c>SELECT ... INTO</c> de PowerScript:
        /// devuelve UNA sola fila (el servicio falla si la consulta devuelve más de una).
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(IDataStore<DynamicModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IDataStore<DynamicModel>>> SelectIntoAsync([FromBody] Dictionary<string, object> queryParams)
        {
            //1- Procesar Parámetros Recibidos
            string sqlEncoded = string.Empty;
            object[]? parametersSelect = new object[queryParams.Count - 1];
            GetQueryParams(queryParams, ref sqlEncoded, ref parametersSelect);
            try
            {
                //2- Llamar al servicio con el SQL en Base64Url y los parámetros
                var result = await iSqlExecutorService.SelectIntoAsync(sqlEncoded, parametersSelect, default);
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
