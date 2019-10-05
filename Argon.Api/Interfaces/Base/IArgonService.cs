using System.Threading.Tasks;

namespace Argon.Api.Interfaces.Base
{
	public interface IArgonService
	{
		Task<bool> Start();

		Task<bool> Stop();
	}
}
