using System;

namespace Argon.Api.Attributes.Environment
{
	[AttributeUsage(AttributeTargets.Property)]
	public class ConfigEnvVariableAttribute : Attribute
	{
		public string EnvName { get; set; }

		public ConfigEnvVariableAttribute(string envName)
		{
			EnvName = envName;
		}
	}
}
