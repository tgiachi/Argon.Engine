using System;
using System.Threading.Tasks;

namespace Argon.Api.Interfaces.Scheduler
{
	public interface IJobSchedulerTask : IDisposable
	{
		Task Execute(params object[] args);
	}
}
