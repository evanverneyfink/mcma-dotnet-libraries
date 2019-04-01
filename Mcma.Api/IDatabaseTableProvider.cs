using Mcma.Core;
using Mcma.Data;

namespace Mcma.Api
{
    public interface IDbTableProvider<T> where T : McmaResource
    {
        IDbTable<T> Table(string tableName);
    }
}
