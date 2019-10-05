using System.Threading.Tasks;
using Argon.Api.Interfaces.Base;

namespace Argon.Api.Interfaces.Manager
{
	public interface IServicesManager
	{
		Task<bool> Start();

		Task<bool> Stop();

		T GetService<T>() where T : IArgonService;
	}
}
