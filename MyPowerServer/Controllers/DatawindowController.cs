using Microsoft.AspNetCore.Mvc;
using DWNet.Data;
using MyPowerServer.Services;
using SnapObjects.Data;

namespace MyPowerServer.Controllers
{
    [Route("api/[controller]/[action]")]
	[ApiController]
	public class DatawindowController : MyControllerBase
    {
       private readonly IDatawindowService _idatawindowService;
        
        public DatawindowController(IDatawindowService idatawindowService)
        {
            _idatawindowService = idatawindowService;
        }  
        //POST api/Datawindow/Retrieve
        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> RetrieveAsync([FromBody] Dictionary<string, object> queryParams)
        {
            //1- Procesar Parámetros Recibidos
            string dwSyntaxEncoded = string.Empty;
            object[]? parametersSelect = new object[queryParams.Count - 1];
            GetQueryParams(queryParams, ref dwSyntaxEncoded, ref parametersSelect);
            try
            {
                //2- Llamar al servicio con el SQL en base64 y los parámetros
                var result = await _idatawindowService.RetrieveAsync(dwSyntaxEncoded, parametersSelect, default);
                return Ok(result);
            }
            catch (Exception ex)
            {
                //3- Control de Errores
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        
        //POST api/Datawindow/Update   
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
                //2- Llamar al servicio con el SQL en base64 y los parámetros
                int result = await _idatawindowService.UpdateAsync(dwSyntaxEncoded, parametersUpdate, default);
                return Ok(result);
            }
            catch (Exception ex)
            {
                //3- Control de Errores
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        //POST api/Datawindow/Cargar 
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
                //2- Llamar al servicio con el SQL en base64 y los parámetros
                var result = await _idatawindowService.CargarAsync(sqlEncoded, parametersSelect, default);
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

