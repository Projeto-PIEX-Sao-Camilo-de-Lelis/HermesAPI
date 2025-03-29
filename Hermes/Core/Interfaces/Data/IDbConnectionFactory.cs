using System.Data;

namespace Hermes.Core.Interfaces.Data
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
