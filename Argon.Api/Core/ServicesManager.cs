using Microsoft.Extensions.Logging;
using Neon.Api.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Argon.Api.Attributes.Services;
using Argon.Api.Data.Services;
using Argon.Api.Interfaces.Base;
using Argon.Api.Interfaces.Manager;
using Argon.Api.Utils;

namespace Neon.Api.Core
{
	public class ServicesManager : IServicesManager
	{
		private readonly ILogger _logger;
		private readonly IArgonManager _argonManager;
		private readonly SortedDictionary<int, List<Type>> _orderedService = new SortedDictionary<int, List<Type>>();
		private readonly Dictionary<Guid, IArgonService> _services = new Dictionary<Guid, IArgonService>();

		public ServicesManager(ILogger<ServicesManager> logger, IArgonManager argonManager)
		{
			_logger = logger;
			_argonManager = argonManager;

			SortServices();
		}

		public async Task<bool> Start()
		{
			var timeBoot = TimeSpan.Zero;

			foreach (var entry in _orderedService)
			{
				foreach (var service in entry.Value)
				{
					var sw = new Stopwatch();
					sw.Start();

					var result = await StartService(service);

					sw.Stop();

					timeBoot += sw.Elapsed;
					if (result.Status == ServiceStatus.Error)
					{
						_logger.LogError($"Error during starting service {service.Name}: {result.Error.Message}");
						_logger.LogError(result.Error, "Error");
					}
				}
			}

			_logger.LogInformation($"{_services.Count} services loaded in {timeBoot.TotalSeconds} seconds");

			return true;
		}

		public async Task<bool> Stop()
		{
			foreach (var entry in _services)
			{
				await entry.Value.Stop();
			}

			return true;
		}

		public T GetService<T>() where T : IArgonService
		{
			return (T)_services.Values.FirstOrDefault(service => service.GetType().FullName == typeof(T).FullName);
		}


		private async Task<ServiceInfo> StartService(Type type)
		{
			var sw = new Stopwatch();
			sw.Start();

			var serviceAttribute = type.GetCustomAttribute<ArgonServiceAttribute>();
			_logger.LogInformation($"Starting service {serviceAttribute.Name} Order = {serviceAttribute.LoadOrder} ");

			var serviceInfo = new ServiceInfo()
			{
				Name = serviceAttribute.Name,
				Description = serviceAttribute.Description
			};


			try
			{
				var service = _argonManager.Resolve(AssemblyUtils.GetInterfaceOfType(type)) as IArgonService;

				await service.Start();

				_services.Add(serviceInfo.Id, service);

				serviceInfo.Status = ServiceStatus.Started;

				sw.Stop();

				_logger.LogInformation($"Service {serviceAttribute.Name} started in {sw.Elapsed.TotalSeconds} seconds ");


				return serviceInfo;
			}
			catch (Exception e)
			{

				serviceInfo.Error = e;
				serviceInfo.Status = ServiceStatus.Error;
				return serviceInfo;
			}

		}

		private void SortServices()
		{
			_argonManager.AvailableServices.ForEach(s =>
			{
				var serviceAttribute = s.GetCustomAttribute<ArgonServiceAttribute>();

				if (!_orderedService.ContainsKey(serviceAttribute.LoadOrder))
					_orderedService.Add(serviceAttribute.LoadOrder, new List<Type>());

				_orderedService[serviceAttribute.LoadOrder].Add(s);

			});
		}
	}
}
