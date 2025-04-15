using Microsoft.AspNetCore.Mvc;

namespace MyPowerServer.Controllers
{
    public class MyControllerBase : ControllerBase
    {
        
        protected void GetQueryParams(Dictionary<string, object> queryParams, ref string sqlEncoded, ref object[] parameters)
        {
            // Validar si el diccionario de parámetros es nulo o tiene menos de un parámetro
            JsonProcessorHelper.GetQueryParams(queryParams, ref sqlEncoded, ref parameters);

        }

     }
}
