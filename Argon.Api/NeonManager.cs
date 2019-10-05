using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Argon.Api.Attributes.NoSql;
using Argon.Api.Attributes.Services;
using Argon.Api.Attributes.WebSocket;
using Argon.Api.Core;
using Argon.Api.Data.Config.Common;
using Argon.Api.Data.Config.Root;
using Argon.Api.Interfaces.Base;
using Argon.Api.Interfaces.Manager;
using Argon.Api.Utils;
using Autofac;
using MediatR;
using MediatR.Pipeline;
using Neon.Api.Core;
using Serilog;
using Serilog.Filters;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;

namespace Argon.Api
{

	/// <summary>
	/// Implementation of Neon Manager 
	/// </summary>
	public class NeonManager : IArgonManager
	{
		private readonly ILogger _logger;
		private IContainer _container;
		private readonly ContainerBuilder _containerBuilder;

		private IServicesManager _servicesManager;
		private readonly IConfigManager _configManager;
		private readonly IFileSystemManager _fileSystemManager;
		private readonly ISecretKeyManager _secretKeyManager;

		public ContainerBuilder ContainerBuilder => _containerBuilder;
		public List<Type> AvailableServices { get; }
		public ArgonConfig Config => _configManager.Configuration;



		public bool IsRunningInDocker { get; }


		public NeonManager()
		{
			ConfigureLogger(null);
			PrintHeader();
			_logger = Log.Logger.ForContext<NeonManager>();
			_logger.Debug($"Pre-loading assemblies");
			var assemblies =  AssemblyUtils.GetAppAssemblies();
			_logger.Debug($"Loaded {assemblies.Count} assemblies");

			AvailableServices = new List<Type>();
			IsRunningInDocker = Environment.GetEnvironmentVariables()["DOTNET_RUNNING_IN_CONTAINER"] != null;
		
			_containerBuilder = new ContainerBuilder();
			_containerBuilder.RegisterBuildCallback(container => { _logger.Debug($"Container is ready"); });

			_configManager = new ConfigManager(_logger, this, _containerBuilder);
			_configManager.LoadConfig();

			_secretKeyManager = new SecretKeyManager(Config.EngineConfig.SecretKey);

			_fileSystemManager = new FileSystemManager(_logger, Config, _secretKeyManager);
			_fileSystemManager.Start();

			ConfigureLogger(_configManager);

		
		}

		private void PrintHeader()
		{
			var framework = Assembly
				.GetEntryAssembly()?
				.GetCustomAttribute<TargetFrameworkAttribute>()?
				.FrameworkName;

			var stats = new
			{                
				OsPlatform = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
				AspDotnetVersion = framework
			};


			Console.WriteLine($@" 
 $$$$$$\                                          
$$  __$$\                                         
$$ /  $$ | $$$$$$\   $$$$$$\   $$$$$$\  $$$$$$$\  
$$$$$$$$ |$$  __$$\ $$  __$$\ $$  __$$\ $$  __$$\ 
$$  __$$ |$$ |  \__|$$ /  $$ |$$ /  $$ |$$ |  $$ |
$$ |  $$ |$$ |      $$ |  $$ |$$ |  $$ |$$ |  $$ |
$$ |  $$ |$$ |      \$$$$$$$ |\$$$$$$  |$$ |  $$ |
\__|  \__|\__|       \____$$ | \______/ \__|  \__|
                    $$\   $$ |                    
                    \$$$$$$  |                    
                     \______/                
:: Home Control v {AssemblyUtils.GetVersion()}
:: Os {stats.OsPlatform}
:: .NET core version {stats.AspDotnetVersion}
                        ");
			Console.WriteLine($"Starting Neon (branch {ThisAssembly.Git.Branch}) {ThisAssembly.Git.Commit} sha: {ThisAssembly.Git.Sha}");
		}
		private void ConfigureLogger(IConfigManager configManager)
		{
			if (_configManager == null)
			{
				Log.Logger = new LoggerConfiguration()
					.Filter.ByExcluding(Matching.FromSource("Microsoft"))
					.Filter
					.ByExcluding(Matching.FromSource("System"))
					.Enrich.FromLogContext()
					.MinimumLevel.Debug()
					.WriteTo.File(new CompactJsonFormatter(), "logs/Argon.log",
						rollingInterval: RollingInterval.Day)
					.WriteTo.Console(
						theme: AnsiConsoleTheme.Code,
						outputTemplate:
						"{Timestamp:HH:mm:ss} [{Level}] [{SourceContext:u3}] {Message}{NewLine}{Exception}")
					.CreateLogger();
			}
			else
			{
				var logConfiguration = new LoggerConfiguration()
					.Filter.ByExcluding(Matching.FromSource("Microsoft"))
					.Filter
					.ByExcluding(Matching.FromSource("System"))
					.Enrich.FromLogContext();

				if (_configManager.Configuration.EngineConfig.Logger.Level == LogLevelEnum.Debug)
					logConfiguration = logConfiguration.MinimumLevel.Debug();

				if (_configManager.Configuration.EngineConfig.Logger.Level == LogLevelEnum.Info)
					logConfiguration = logConfiguration.MinimumLevel.Information();

				if (_configManager.Configuration.EngineConfig.Logger.Level == LogLevelEnum.Warning)
					logConfiguration = logConfiguration.MinimumLevel.Warning();

				if (_configManager.Configuration.EngineConfig.Logger.Level == LogLevelEnum.Error)
					logConfiguration = logConfiguration.MinimumLevel.Error();

				if (!string.IsNullOrEmpty(_configManager.Configuration.EngineConfig.Logger.LogDirectory))
					logConfiguration = logConfiguration.WriteTo.File(new CompactJsonFormatter(), _fileSystemManager.BuildFilePath(Path.Combine(_configManager.Configuration.EngineConfig.Logger.LogDirectory, "Argon.log")),
						rollingInterval: RollingInterval.Day);

				logConfiguration = logConfiguration.WriteTo.Console(
					theme: AnsiConsoleTheme.Code,
					outputTemplate:
					"{Timestamp:HH:mm:ss} [{Level}] [{SourceContext:u3}] {Message}{NewLine}{Exception}");

				Log.Logger = logConfiguration.CreateLogger();
			}
		}

		public bool Init()
		{
			_logger.Debug($"Registering Container");
			_containerBuilder.Register(c => _container).AsSelf();

			_logger.Debug($"Registering NeonManager");
			_containerBuilder.Register(n => this).As<IArgonManager>().SingleInstance();

			_logger.Debug($"Registering Config Manager");
			_containerBuilder.Register(n => _configManager).As<IConfigManager>().SingleInstance();

			_logger.Debug($"Registering Secret Keys Manager");
			_containerBuilder.Register(n => _secretKeyManager).As<ISecretKeyManager>().SingleInstance();

			_logger.Debug($"Registering FileSystem Manager");
			_containerBuilder.Register(n => _fileSystemManager).As<IFileSystemManager>().SingleInstance();

			_logger.Debug($"Registering Services Manager");
			_containerBuilder.RegisterType<ServicesManager>().As<IServicesManager>().SingleInstance();


			_logger.Debug($"Registering Mediator");
			RegisterMediator();



			_logger.Debug($"Registering NoSQL connectors");
			RegisterNoSqlConnectors();



			_logger.Debug("Registering WebSocket hubs");
			RegisterWebSockets();



			ScanTypes();

		

			return true;
		}

		private void ScanTypes()
		{
			_logger.Debug($"Scan for services");
			AssemblyUtils.GetAttribute<ArgonServiceAttribute>().ForEach(s =>
			{
				_logger.Debug($"Registering service {s.Name}");

				_containerBuilder.RegisterType(s).As(AssemblyUtils.GetInterfaceOfType(s)).SingleInstance();
				AvailableServices.Add(s);
			});
		}



		private void RegisterWebSockets()
		{
			_logger.Debug($"Scan for WebSockets hub");
			AssemblyUtils.GetAttribute<WebSocketHubAttribute>().ForEach(w =>
			{
				_logger.Debug($"Registering WebSocket {w.Name}");
				_containerBuilder.RegisterType(w).SingleInstance();
			});
		}

		private void RegisterMediator()
		{
			ContainerBuilder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly).AsImplementedInterfaces();

			AssemblyUtils.GetAppAssemblies().ForEach(a =>
			{
				ContainerBuilder
					.RegisterAssemblyTypes(a)
					.AsClosedTypesOf(typeof(IRequestHandler<,>))
					.AsImplementedInterfaces().SingleInstance(); ;

				ContainerBuilder
					.RegisterAssemblyTypes(a)
					.AsClosedTypesOf(typeof(INotificationHandler<>))
					.AsImplementedInterfaces().SingleInstance();
			});

			ContainerBuilder.RegisterGeneric(typeof(RequestPostProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
			ContainerBuilder.RegisterGeneric(typeof(RequestPreProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));

			ContainerBuilder.Register<ServiceFactory>(ctx =>
			{
				var c = ctx.Resolve<IComponentContext>();
				return t => c.Resolve(t);
			});
		}

		private void RegisterNoSqlConnectors()
		{
			AssemblyUtils.GetAttribute<NoSqlConnectorAttribute>().ForEach(t =>
				{
					ContainerBuilder.RegisterType(t).InstancePerDependency();
				});
		}

		public async Task Start()
		{
			await _servicesManager.Start();
		}

		/// <summary>
		/// Build container
		/// </summary>
		/// <returns></returns>
		public IContainer Build()
		{

			_container = _containerBuilder.Build();

			_servicesManager = _container.Resolve<IServicesManager>();

			return _container;
		}

		public async Task Shutdown()
		{

			await _servicesManager.Stop();
			_fileSystemManager.Stop();
		}

		public T ResolveInContext<T>()
		{
			return _container.BeginLifetimeScope().Resolve<T>();
		}

		public T GetService<T>() where T : IArgonService
		{
			return _servicesManager.GetService<T>();
		}

		public T Resolve<T>()
		{
			
			return _container.Resolve<T>();
		}

		public object Resolve(Type t)
		{
			return _container.Resolve(t);
		}
	}
}
