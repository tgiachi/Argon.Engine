using YamlDotNet.Serialization;

namespace Argon.Api.Data.Config.Root
{
	public class ArgonConfig
	{
		[YamlMember(Alias = "engine")]
		public EngineConfig EngineConfig { get; set; }

	

		public ArgonConfig()
		{
			EngineConfig = new EngineConfig();
	
		}
	}
}
