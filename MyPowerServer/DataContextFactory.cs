using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace MyPowerServer
{
    /// <summary>
    /// Fábrica de contextos de datos. Permite que el MISMO servidor hable con varias bases de
    /// datos: el cliente indica a cuál quiere conectarse mediante la cabecera HTTP <c>profile</c>,
    /// y esta fábrica devuelve el <see cref="DefaultDataContext"/> con la cadena de conexión
    /// correspondiente (leída de appsettings). Sólo se admiten perfiles de una lista blanca,
    /// por seguridad. Se registra en Program.cs y la usa el factory Scoped del contenedor DI.
    /// </summary>
    public class DataContextFactory
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpAccessor;

        // Nombre de la cabecera HTTP donde el cliente indica la BD destino.
        private const string PROFILE_HEADER_NAME = "profile";

        // Lista blanca de perfiles permitidos. Evita que alguien pida una conexión arbitraria.
        private readonly string[] _allowedProfiles = { "PersonDemo03", "PBDemoDB2022" };

        // IHttpContextAccessor nos da acceso a la petición HTTP en curso (para leer la cabecera).
        public DataContextFactory(IConfiguration config, IHttpContextAccessor httpAccessor)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpAccessor = httpAccessor ?? throw new ArgumentNullException(nameof(httpAccessor));
        }

        /// <summary>
        /// Punto de entrada: averigua el perfil a partir de la cabecera y crea el contexto.
        /// </summary>
        public DefaultDataContext GetDataContext()
        {
            string profile = DetermineProfileFromHeader();
            return CreateDataContext(profile);
        }

        /// <summary>
        /// Crea un <see cref="DefaultDataContext"/> para el <paramref name="profile"/> indicado,
        /// resolviendo su cadena de conexión en appsettings (sección ConnectionStrings).
        /// </summary>
        public DefaultDataContext CreateDataContext(string profile)
        {
            if (string.IsNullOrEmpty(profile))
            {
                throw new ArgumentException("El perfil de conexión no puede estar vacío", nameof(profile));
            }

            var connectionString = _config.GetConnectionString(profile);

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception($"No se encontró la cadena de conexión para el perfil '{profile}'");
            }

            return new DefaultDataContext(connectionString);
        }

        // Lee y valida la cabecera 'profile' de la petición actual.
        private string DetermineProfileFromHeader()
        {
            // Sin contexto HTTP o sin cabecera 'profile' → no sabemos a qué BD ir: cortamos.
            if (_httpAccessor.HttpContext == null || !_httpAccessor.HttpContext.Request.Headers.TryGetValue(PROFILE_HEADER_NAME, out var profileHeader))
            {
                throw new UnauthorizedAccessException("El header 'profile' es obligatorio.");
            }

            string requestedProfile = profileHeader.ToString();

            // El perfil debe estar en la lista blanca; si no, lo rechazamos.
            if (!Array.Exists(_allowedProfiles, p => p == requestedProfile))
            {
                throw new UnauthorizedAccessException($"Perfil no permitido: {requestedProfile}");
            }

            return requestedProfile;
        }
    }
}
