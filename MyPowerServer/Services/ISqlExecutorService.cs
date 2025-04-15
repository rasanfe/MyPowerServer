using SnapObjects.Data;

namespace MyPowerServer.Services
{
    public interface ISqlExecutorService
    {
        Task<int> InsertAsync(string sqlEncrypted, object[] parametersInsert, CancellationToken cancellationToken);

        Task UpdateAsync(string sqlEncrypted, object[] parametersUpdate, CancellationToken cancellationToken);

        Task DeleteAsync(string sqlEncrypted, object[] parametersDelete, CancellationToken cancellationToken);

        Task<IList<DynamicModel>> SelectIntoAsync(string sqlEncrypted, object[] parametersSelect, CancellationToken cancellationToken);

    }
}
