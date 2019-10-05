using Microsoft.Extensions.Logging;

using Neon.Api.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Argon.Api.Attributes.NoSql;
using Argon.Api.Attributes.Services;
using Argon.Api.Interfaces.Base;
using Argon.Api.Interfaces.Manager;
using Argon.Api.Interfaces.NoSql;
using Argon.Api.Interfaces.Services;
using Argon.Api.Utils;

namespace Neon.Engine.Services
{
	[ArgonService("NoSql Service", "Manage NoSQL connectors", 1)]
	public class NoSqlService : INoSqlService
	{
		private readonly Dictionary<string, Type> _nosqlConnectors = new Dictionary<string, Type>();
		private readonly ILogger _logger;
		private readonly IArgonManager _argonManager;

		public NoSqlService(ILogger<NoSqlService> logger, IArgonManager argonManager)
		{
			_logger = logger;
			_argonManager = argonManager;
		}

		public Task<bool> Start()
		{
			AssemblyUtils.GetAttribute<NoSqlConnectorAttribute>().ForEach(t =>
			{
				var attr = t.GetCustomAttribute<NoSqlConnectorAttribute>();
				_nosqlConnectors.Add(attr.Name, t);
				_logger.LogInformation($"Adding NoSql connector {attr.Name} [{t.Name}]");
			});

			return Task.FromResult(true);
		}

		public Task<bool> Stop()
		{
			_nosqlConnectors.Clear();
			return Task.FromResult(true);
		}

		public INoSqlConnector GetNoSqlConnector(string name)
		{
			return _argonManager.Resolve(_nosqlConnectors[name]) as INoSqlConnector;
		}
	}
}
