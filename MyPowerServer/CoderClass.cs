using System;
using System.Text;

namespace MyPowerServer
{
    public static class CoderClass
    {
        public static string Base64UrlEncode(string source)
        {
            // Convertir el texto de entrada a un arreglo de bytes
            byte[] bytes = Encoding.UTF8.GetBytes(source);
            
            // Convertir los bytes a Base64
            string encodedText = Convert.ToBase64String(bytes);
            
            // Reemplazar caracteres para ajustarse a Base64URL
            encodedText = encodedText.Replace("+", "-").Replace("/", "_").Replace("=", "");
            
            return encodedText;
        }
        
        public static string Base64UrlDecode(string source)
        {
            // Reemplazar caracteres de Base64URL por los estándar de Base64
            source = source.Replace("-", "+").Replace("_", "/");
            while (source.Length % 4 != 0)
            {
                source += "="; // Ajustar padding para Base64
            }

            // Convertir de Base64 a un arreglo de bytes
            byte[] bytes = Convert.FromBase64String(source);
            
            // Convertir los bytes a texto UTF-8
            string decodedText = Encoding.UTF8.GetString(bytes);
            
            return decodedText;
        }
    }
}
