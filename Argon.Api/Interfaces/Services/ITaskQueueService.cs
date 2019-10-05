using System;
using System.Threading.Tasks;
using Argon.Api.Interfaces.Base;

namespace Argon.Api.Interfaces.Services
{
	public interface ITaskQueueService : IArgonService
	{
		int QueueCount { get; }

		int RunningTaskCount { get; }

		bool Queue(Func<Task> futureTask);


	}
}
