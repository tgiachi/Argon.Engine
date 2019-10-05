using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Argon.Api.Interfaces.Entities;

namespace Argon.Api.Interfaces.NoSql
{
	public interface INoSqlConnector
	{
		Task<bool> Start();

		Task<bool> Stop();

		Task<bool> Configure(string connectionString);

		List<TEntity> List<TEntity>(string collectionName) where TEntity : class, IArgonEntity;

		List<object> FindAllGeneric(string collectionName);

		IQueryable<TEntity> Query<TEntity>(string collectionName) where TEntity : class, IArgonEntity;

		TEntity Insert<TEntity>(string collectionName, TEntity obj) where TEntity : class, IArgonEntity;

		TEntity Update<TEntity>(string collectionName, TEntity obj) where TEntity : class, IArgonEntity;

		bool Delete<TEntity>(string collectionName, TEntity obj) where TEntity : class, IArgonEntity;
	}
}
