using System;
using SnapObjects.Data;
using SnapObjects.Data.SqlServer;

namespace MyPowerServer
{
    public class DataContextBase : SqlServerDataContext
    {
        public DataContextBase(string connectionString)
            : this(new SqlServerDataContextOptions<DataContextBase>(connectionString))
        {
        }

        public DataContextBase(IDataContextOptions<DataContextBase> options)
            : base(options)
        {
        }

        public DataContextBase(IDataContextOptions options)
            : base(options)
        {
        }
    }
}
