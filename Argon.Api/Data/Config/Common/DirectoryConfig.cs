using YamlDotNet.Serialization;

namespace Argon.Api.Data.Config.Common
{
	public class DirectoryConfig
	{
		[YamlMember(Alias = "name")]
		public string DirectoryName { get; set; }
	}
}
