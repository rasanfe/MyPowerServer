using System.Globalization;
using System.Text.Json;

namespace MyPowerServer
{
    /// <summary>
    /// Ayudante para procesar los parámetros que llegan en el cuerpo de las peticiones.
    /// El cliente manda un diccionario donde el PRIMER valor es el SQL/sintaxis (en Base64Url)
    /// y los siguientes son los parámetros del SELECT/UPDATE. Aquí los separamos y, sobre todo,
    /// convertimos cada parámetro de su forma "cruda" de JSON (<see cref="JsonElement"/>) al tipo
    /// .NET adecuado (int, decimal, DateTime, etc.) para que el SQL los reciba con el tipo correcto.
    /// Es estática porque sólo transforma datos, no guarda estado.
    /// </summary>
    public static class JsonProcessorHelper
    {
        #region Public Methods

        /// <summary>
        /// Parte <paramref name="queryParams"/> en dos: el SQL/sintaxis codificado
        /// (<paramref name="encodedSql"/>, el primer valor) y el array de
        /// <paramref name="parameters"/> ya convertidos a tipos .NET (el resto).
        /// Lanza <see cref="ArgumentException"/> si no llega el SQL o el diccionario viene vacío.
        /// </summary>
        public static void GetQueryParams(
           Dictionary<string, object> queryParams,
           ref string encodedSql,
           ref object[] parameters)
        {
            if (queryParams == null || queryParams.Count < 1)
            {
                throw new ArgumentException("Invalid parameters. The SQL query and at least one parameter are required.");
            }

            // El array de parámetros lleva una posición menos que el diccionario: el primer
            // elemento es el SQL, no un parámetro.
            parameters = new object[queryParams.Count - 1];

            int paramIndex = 0;
            bool sqlFound = false;

            foreach (var param in queryParams)
            {
                if (paramIndex == 0)
                {
                    // El primer valor es el SQL codificado.
                    encodedSql = param.Value?.ToString() ?? string.Empty;
                    sqlFound = true;
                }
                else
                {
                    // El resto son parámetros: los convertimos de JsonElement a su tipo .NET.
                    // El valor convertido puede ser null (JSON null => SQL NULL); el array lo admite.
                    parameters[paramIndex - 1] = ConvertIfJsonElement(param.Value)!;
                }
                paramIndex++;
            }

            // Nos aseguramos de que el SQL venía y no estaba vacío.
            if (!sqlFound || string.IsNullOrEmpty(encodedSql))
            {
                throw new ArgumentException("SQL encoded query is missing or invalid.");
            }
        }

        #endregion

        #region Private Methods

        // System.Text.Json deja los valores como JsonElement (un "envoltorio" del JSON sin tipar).
        // Si lo es, lo convertimos al tipo .NET real; si no, lo dejamos tal cual.
        private static object? ConvertIfJsonElement(object? value)
        {
            return value is JsonElement jsonElement ? ConvertJsonElementToClrValue(jsonElement) : value;
        }

        // Helper auxiliar (no usado por el flujo actual): deserializa un JSON a diccionario.
        // Se deja como utilidad por si se necesita procesar cuerpos JSON completos.
        private static Dictionary<string, object> DeserializeJsonToDictionary(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                throw new ArgumentNullException(nameof(jsonString), "JSON string cannot be null or empty");
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(
                    jsonString,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    })
                    ?? throw new JsonException("JSON deserialization returned null.");
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Failed to deserialize JSON string: {ex.Message}", ex);
            }
        }

        // Traduce un JsonElement al tipo .NET que le corresponde según su "ValueKind".
        // El switch de expresión (=>) es la forma moderna y compacta de un gran if/else.
        private static object? ConvertJsonElementToClrValue(JsonElement jsonElement) => jsonElement.ValueKind switch
        {
            JsonValueKind.String => ParseStringValue(jsonElement.GetString()),
            JsonValueKind.Number => ParseNumberValue(jsonElement),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Object => jsonElement.ToString(),
            JsonValueKind.Array => ConvertJsonArray(jsonElement),
            _ => null
        };

        // Una cadena JSON puede ser realmente una fecha/hora; intentamos reconocerla por formato
        // (fecha+hora, hora, o fecha sola) antes de quedarnos con el texto plano.
        private static object? ParseStringValue(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (TryParseDateTime(value, out DateTime dateTime))
            {
                return dateTime;
            }

            if (TryParseTimeSpan(value, out TimeSpan timeSpan))
            {
                return timeSpan;
            }

            if (TryParseDateOnly(value, out DateOnly dateOnly))
            {
                return dateOnly;
            }

            return value;
        }

        // Formatos de fecha+hora aceptados (formato español dd-MM-yyyy). InvariantCulture para
        // que no dependa de la configuración regional del servidor.
        private static bool TryParseDateTime(string value, out DateTime dateTime)
        {
            string[] formats = {
                "dd-MM-yyyy HH:mm:ss,ffffff",
                "dd-MM-yyyy HH:mm:ss",
                "dd-MM-yyyy HH:mm"
            };

            return DateTime.TryParseExact(
                value,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out dateTime);
        }

        // Formatos de hora suelta (TimeSpan).
        private static bool TryParseTimeSpan(string value, out TimeSpan timeSpan)
        {
            string[] formats = {
                "HH:mm:ss,ffffff",
                "HH:mm:ss",
                "HH:mm"
            };

            return TimeSpan.TryParseExact(
                value,
                formats,
                CultureInfo.InvariantCulture,
                out timeSpan);
        }

        // Fecha sin hora (DateOnly, tipo introducido en .NET 6).
        private static bool TryParseDateOnly(string value, out DateOnly dateOnly)
        {
            return DateOnly.TryParseExact(
                value,
                "dd-MM-yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out dateOnly);
        }

        // Un número JSON puede caber en int, long o decimal; probamos de menor a mayor
        // precisión para devolver el tipo más ajustado (y double como último recurso).
        private static object ParseNumberValue(JsonElement jsonElement)
        {
            if (jsonElement.TryGetInt32(out int intValue))
            {
                return intValue;
            }

            if (jsonElement.TryGetInt64(out long longValue))
            {
                return longValue;
            }

            if (jsonElement.TryGetDecimal(out decimal decimalValue))
            {
                return decimalValue;
            }

            return jsonElement.GetDouble();
        }

        // Convierte un array JSON recursivamente, elemento a elemento.
        private static object[] ConvertJsonArray(JsonElement jsonElement)
        {
            var items = new List<object>();

            foreach (var item in jsonElement.EnumerateArray())
            {
                items.Add(ConvertJsonElementToClrValue(item)!);
            }

            return items.ToArray();
        }

        #endregion
    }
}
