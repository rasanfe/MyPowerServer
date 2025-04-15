using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

namespace MyPowerServer
{
    public static class JsonProcessorHelper
    {
        #region Public Methods
        
         public static void GetQueryParams(
           Dictionary<string, object> queryParams,
           ref string encodedSql,
           ref object[] parameters)
        {
            if (queryParams == null || queryParams.Count < 1)
            {
                throw new ArgumentException("Invalid parameters. The SQL query and at least one parameter are required.");
            }

            // Create a new parameters array with the correct size
            parameters = new object[queryParams.Count - 1];
            
            // Process each query parameter
            int paramIndex = 0;
            bool sqlFound = false;
            
            foreach (var param in queryParams)
            {
                if (paramIndex == 0)
                {
                    // The first parameter is expected to be the encoded SQL
                    encodedSql = param.Value?.ToString();
                    sqlFound = true;
                }
                else
                {
                    // The remaining parameters are converted and stored
                    parameters[paramIndex - 1] = ConvertIfJsonElement (param.Value);
                }
                paramIndex++;
            }

            // Ensure the SQL was found and is valid
            if (!sqlFound || string.IsNullOrEmpty(encodedSql))
            {
                throw new ArgumentException("SQL encoded query is missing or invalid.");
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private static object ConvertIfJsonElement (object value)
        {
            return value is JsonElement jsonElement ? ConvertJsonElementToClrValue (jsonElement) : value;
        }
        
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
                    });
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Failed to deserialize JSON string: {ex.Message}", ex);
            }
        }
        private static object ConvertJsonElementToClrValue (JsonElement jsonElement) => jsonElement.ValueKind switch
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
        
        private static object ParseStringValue(string value)
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
        
        private static bool TryParseDateOnly(string value, out DateOnly dateOnly)
        {
            return DateOnly.TryParseExact(
                value,
                "dd-MM-yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out dateOnly);
        }
        
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
        
        private static object[] ConvertJsonArray(JsonElement jsonElement)
        {
            var items = new List<object>();
            
            foreach (var item in jsonElement.EnumerateArray())
            {
                items.Add(ConvertJsonElementToClrValue (item));
            }

            return items.ToArray();
        }
        
        #endregion
    }
}