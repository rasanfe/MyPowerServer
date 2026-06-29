using System.Text;

namespace MyPowerServer
{
    /// <summary>
    /// Utilidad estática para codificar/decodificar en <b>Base64Url</b> (Base64 apto para URLs).
    /// La usan los servicios para recibir SQL y sintaxis sin que se rompan por caracteres
    /// conflictivos. Es estática porque no guarda estado: entra texto, sale texto.
    /// </summary>
    public static class CoderClass
    {
        /// <summary>Texto plano → Base64Url.</summary>
        public static string Base64UrlEncode(string source)
        {
            // Texto → bytes (UTF-8).
            byte[] bytes = Encoding.UTF8.GetBytes(source);

            // Bytes → Base64 estándar.
            string encodedText = Convert.ToBase64String(bytes);

            // Base64 → Base64URL: + → -, / → _, y fuera el relleno '='.
            encodedText = encodedText.Replace("+", "-").Replace("/", "_").Replace("=", "");

            return encodedText;
        }

        /// <summary>Base64Url → texto plano.</summary>
        public static string Base64UrlDecode(string source)
        {
            // Base64URL → Base64 estándar.
            source = source.Replace("-", "+").Replace("_", "/");
            while (source.Length % 4 != 0)
            {
                source += "="; // Reponemos el relleno hasta que la longitud sea múltiplo de 4.
            }

            // Base64 → bytes.
            byte[] bytes = Convert.FromBase64String(source);

            // Bytes → texto (UTF-8).
            string decodedText = Encoding.UTF8.GetString(bytes);

            return decodedText;
        }
    }
}
