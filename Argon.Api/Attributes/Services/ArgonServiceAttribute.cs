using System;

namespace Argon.Api.Attributes.Services
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
	public class ArgonServiceAttribute : Attribute
	{
		public string Name { get; set; }

		public string Description { get; set; }

		public int LoadOrder { get; set; } = 5;

		public ArgonServiceAttribute(string name, string description, int order = 5)
		{
			Name = name;
			Description = description;
			LoadOrder = order;
		}

	}
}
