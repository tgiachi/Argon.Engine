using System;

namespace Argon.Api.Interfaces.Entities
{
	public interface IArgonEntity
	{
		string Id { get; set; }

		DateTime CreateDateTime { get; set; }

		DateTime UpdateDateTime { get; set; }
	}
}
