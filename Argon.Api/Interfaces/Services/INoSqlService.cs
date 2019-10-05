using Argon.Api.Interfaces.Base;
using Argon.Api.Interfaces.NoSql;

namespace Argon.Api.Interfaces.Services
{
	public interface INoSqlService : IArgonService
	{
		INoSqlConnector GetNoSqlConnector(string name);
	}
}
