using SnapObjects.Data;
using DWNet.Data;

namespace MyPowerServer.Services
{
    public interface IDatawindowService
    {
        Task<object> RetrieveAsync(string dwSyntaxEncoded, object[] parametersSelect, CancellationToken cancellationToken);
        Task<int> UpdateAsync(string dwSyntaxEncoded, object[] jsonUpdate, CancellationToken cancellationToken);
        Task<object> CargarAsync(string sqlEncoded, object[] parametersSelect, CancellationToken cancellationToken);
    }
}
