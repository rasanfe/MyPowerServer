using System;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using SnapObjects.Data.SqlServer;

namespace MyPowerServer
{
    public class DataContextFactory
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpAccessor;
        
        // Constante para el nombre del header
        private const string PROFILE_HEADER_NAME = "profile";
        
        // Valores permitidos para el perfil
        private readonly string[] _allowedProfiles = { "PersonDemo03", "PBDemoDB2022" };
        
        public DataContextFactory(IConfiguration config, IHttpContextAccessor httpAccessor)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpAccessor = httpAccessor ?? throw new ArgumentNullException(nameof(httpAccessor));
        }
        
        public DefaultDataContext GetDataContext()
        {
            string profile = DetermineProfileFromHeader();
            return CreateDataContext(profile);
        }
        
        public DefaultDataContext CreateDataContext(string profile)
        {
            if (string.IsNullOrEmpty(profile))
            {
                throw new ArgumentException("El perfil de conexión no puede estar vacío", nameof(profile));
            
            }

            var connectionString = _config.GetConnectionString(profile);
            
            if (string.IsNullOrEmpty(connectionString))
            {
                //throw new InvalidOperationException($"No se encontró la cadena de conexión para el perfil '{profile}'");
                throw new Exception($"No se encontró la cadena de conexión para el perfil '{profile}'");
            }

            return new DefaultDataContext(connectionString);
        }
        
        private string DetermineProfileFromHeader()
        {
            if (_httpAccessor.HttpContext == null || !_httpAccessor.HttpContext.Request.Headers.TryGetValue(PROFILE_HEADER_NAME, out var profileHeader))
            {
                throw new UnauthorizedAccessException("El header 'profile' es obligatorio.");
            }

            string requestedProfile = profileHeader.ToString();

            if (!Array.Exists(_allowedProfiles, p => p == requestedProfile))
            {
                throw new UnauthorizedAccessException($"Perfil no permitido: {requestedProfile}");
            }

            return requestedProfile;
        }

    }
}