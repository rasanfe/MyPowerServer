using Microsoft.AspNetCore.Mvc;
using MyPowerServer.Services;
using DWNet.Data;
using SnapObjects.Data;

namespace MyPowerServer.Controllers
{
    [Route("api/[controller]/[action]")]
	[ApiController]
	public class SqlExecutorController : MyControllerBase
    {
        private readonly ISqlExecutorService _iSqlExecutorService;
        
        public SqlExecutorController(ISqlExecutorService iSqlExecutorService)
        {
            _iSqlExecutorService = iSqlExecutorService;
        }
        
        //POST api/SqlExecutor/Insert
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
                //2- Llamar al servicio con el SQL en base64 y los parámetros
                var retorno = await _iSqlExecutorService.InsertAsync(sqlEncoded, parametersInsert, default);
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                //3- Control de Errores
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        //PATCH api/SqlExecutor/Update
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
                //2- Llamar al servicio con el SQL en base64 y los parámetros
                await _iSqlExecutorService.UpdateAsync(sqlEncoded, parametersUpdate, default);
                return Ok(true);
            }
            catch (Exception ex)
            {
                //3- Control de Errores
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        //DELETE api/SqlExecutor/Delete
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
                //2- Llamar al servicio con el SQL en base64 y los parámetros
                await _iSqlExecutorService.DeleteAsync(sqlEncoded, parametersDelete, default);
                return Ok(true);
            }
            catch (Exception ex)
            {
                //3- Control de Errores
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        //POST api/Cursor/SelectInto  
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
                //2- Llamar al servicio con el SQL en base64 y los parámetros
                var result = await _iSqlExecutorService.SelectIntoAsync(sqlEncoded, parametersSelect, default);
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
