using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Argon.Api.Attributes.NoSql;
using Argon.Api.Interfaces.Entities;
using Argon.Api.Interfaces.Manager;
using Argon.Api.Interfaces.NoSql;
using JsonFlatFileDataStore;
using Microsoft.Extensions.Logging;

namespace Argon.Engine.NoSqlConnectors
{
	[NoSqlConnector("flat_json_db")]
	public class FlatJsonDbConnector : INoSqlConnector
	{
		private readonly IFileSystemManager _fileSystemManager;
		private ILogger _logger;
		private readonly object _databaseLock = new object();
		private IDataStore _dataStore;

		public FlatJsonDbConnector(IFileSystemManager fileSystemManager, ILogger<FlatJsonDbConnector> logger)
		{
			_logger = logger;
			_fileSystemManager = fileSystemManager;
		}

		public Task<bool> Start()
		{
			return Task.FromResult(true);
		}

		public Task<bool> Stop()
		{
			_dataStore.Dispose();
			return Task.FromResult(true);
		}

		public Task<bool> Configure(string connectionString)
		{
			lock (_databaseLock)
			{

				connectionString = _fileSystemManager.BuildFilePath(connectionString);
				CheckDatabaseDirectory(connectionString);
				_dataStore = new DataStore(connectionString, keyProperty: nameof(IArgonEntity.Id));
			}

			return Task.FromResult(true);
		}

		public List<TEntity> List<TEntity>(string collectionName) where TEntity : class, IArgonEntity
		{
			return _dataStore.GetCollection<TEntity>(collectionName).AsQueryable().ToList();
		}

		public List<object> FindAllGeneric(string collectionName)
		{
			return _dataStore.GetCollection<object>(collectionName).AsQueryable().ToList();
		}

		public IQueryable<TEntity> Query<TEntity>(string collectionName) where TEntity : class, IArgonEntity
		{
			return _dataStore.GetCollection<TEntity>(collectionName).AsQueryable().AsQueryable();
		}

		public TEntity Insert<TEntity>(string collectionName, TEntity obj) where TEntity : class, IArgonEntity
		{
			_dataStore.GetCollection<TEntity>(collectionName).InsertOne(obj);

			return obj;
		}

		public TEntity Update<TEntity>(string collectionName, TEntity obj) where TEntity : class, IArgonEntity
		{
			_dataStore.GetCollection<TEntity>(collectionName).UpdateOne(obj.Id, obj);
			return obj;
		}

		public bool Delete<TEntity>(string collectionName, TEntity obj) where TEntity : class, IArgonEntity
		{
			return _dataStore.GetCollection<TEntity>(collectionName).DeleteOne(obj.Id);
		}

		private void CheckDatabaseDirectory(string connectionString)
		{
			var directory = Path.GetDirectoryName(connectionString);
			_fileSystemManager.CreateDirectory(directory);
		}
	}
}
