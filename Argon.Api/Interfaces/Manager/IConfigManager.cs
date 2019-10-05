using Argon.Api.Data.Config.Root;

namespace Argon.Api.Interfaces.Manager
{
	public interface IConfigManager
	{
		ArgonConfig Configuration { get; }

		bool LoadConfig();

		void SaveConfig();
	}
}
