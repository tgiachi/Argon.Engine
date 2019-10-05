using System;

namespace Argon.Api.Utils
{
	public static class EntitiesUtils
	{
		public static string GenerateId()
		{
			return Guid.NewGuid().ToString().Replace("-", "");
		}
	}
}
