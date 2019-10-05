using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Argon.Api;
using Argon.Api.Interfaces.Manager;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Argon.Web
{
	public class Program
	{
		public static IArgonManager ArgonManager { get; set; }

		public static void Main(string[] args)
		{
			try
			{
				CreateWebHostBuilder(args).Run();
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}

		public static IWebHost CreateWebHostBuilder(string[] args)
		{

			ArgonManager = new NeonManager();

			var host = WebHost.CreateDefaultBuilder(args)
				.ConfigureKestrel(opts =>
				{
					opts.ListenAnyIP(5000, options => options.Protocols = HttpProtocols.Http1AndHttp2);
				})
				.UseStartup<Startup>()
				.UseLibuv()
				.UseIISIntegration()
				.UseSerilog();

			return host.Build();
		}
	}
}
