﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Argon.Api.Attributes.Scheduler;
using Argon.Api.Attributes.Services;
using Argon.Api.Data.Scheduler;
using Argon.Api.Interfaces.Manager;
using Argon.Api.Interfaces.Scheduler;
using Argon.Api.Interfaces.Services;
using Argon.Api.Utils;
using FluentScheduler;
using Microsoft.Extensions.Logging;

namespace Argon.Engine.Services
{
	[ArgonService("Scheduler Service", "Manage job", 1)]
	public class SchedulerService : ISchedulerService
	{
		private readonly List<IJobSchedulerTask> _jobs = new List<IJobSchedulerTask>();
		private readonly ILogger _logger;
		private readonly IArgonManager _argonManager;


		public SchedulerService(ILogger<SchedulerService> logger, IArgonManager argonManager)
		{
			JobsInfo = new List<JobInfo>();
			_logger = logger;
			_argonManager = argonManager;
		}

		public List<JobInfo> JobsInfo { get; set; }

		public Task<bool> Start()
		{
			_logger.LogInformation("Initializing Job Scheduler");
			JobManager.Initialize(BuildRegistry());

			JobManager.JobStart += info => { _logger.LogDebug($"Starting job {info.Name}"); };
			JobManager.JobException += info =>
			{
				var jobInfo = JobsInfo.FirstOrDefault(j => j.JobName == info.Name);
				jobInfo.HaveError = true;
				jobInfo.Exception = info.Exception;
				_logger.LogError($"Error during execute job {info.Name} => {info.Exception}");
			};
			JobManager.JobStart += info =>
			{
				var jobInfo = JobsInfo.FirstOrDefault(j => j.JobName == info.Name);
				jobInfo.LastExecution = info.StartTime;
			};

			JobManager.JobEnd += info =>
			{
				var jobInfo = JobsInfo.FirstOrDefault(j => j.JobName == info.Name);
				jobInfo.LastExecution = info.NextRun.Value;
			};

			JobManager.Start();

			AddPolling(GC.Collect, "GC", SchedulerServicePollingEnum.NormalPolling);

			return Task.FromResult(true);
		}

		public void AddJob(Action job, int seconds, bool startNow)
		{
			AddJob(job, new StackTrace().GetFrame(1).GetMethod().DeclaringType.Name, seconds, startNow);
		}

		public void AddJob(Action job, string name, int seconds, bool startNow)
		{
			if (!startNow)
				JobManager.AddJob(job, schedule => schedule.WithName(name).ToRunEvery(seconds).Seconds());
			else
				JobManager.AddJob(job, schedule => schedule.WithName(name).ToRunNow().AndEvery(seconds).Seconds());

			JobsInfo.Add(new JobInfo
			{
				JobId = Guid.NewGuid(),
				JobName = name,
				JobType = name.EndsWith("_POLLING") ? JobTypeEnum.Polling : JobTypeEnum.Job,
				Seconds = seconds,
				StartNow = startNow,
				NextExecution = DateTime.Now.AddSeconds(seconds).Date
			});
		}

		public void AddJob(Action job, string name, int hours, int minutes)
		{
			JobManager.AddJob(job, schedule => schedule.WithName(name).ToRunEvery(1).Days().At(hours, minutes));

			JobsInfo.Add(new JobInfo
			{
				JobId = Guid.NewGuid(),
				JobName = name,
				JobType = name.EndsWith("_POLLING") ? JobTypeEnum.Polling : JobTypeEnum.Job,
				Seconds = 0,
				NextExecution = DateTime.Now.AddSeconds(1).Date
			});
		}

		public void AddPolling(Action job, string name, SchedulerServicePollingEnum pollingType)
		{
			_logger.LogDebug($"Adding polling {name} [{pollingType}]");
			AddJob(job, $"{name.ToUpper()}_POLLING", (int)pollingType, false);
		}

		public void AddPolling(Action job, string name, int seconds)
		{
			_logger.LogDebug($"Adding polling {name} [{seconds} seconds]");
			AddJob(job, $"{name.ToUpper()}_POLLING", seconds, false);
		}

		public Task<bool> Stop()
		{
			JobManager.StopAndBlock();
			_jobs.ForEach(j => { j.Dispose(); });

			return Task.FromResult(true);
		}

		private Registry BuildRegistry()
		{
			var registry = new Registry();

			AssemblyUtils.GetAttribute<SchedulerJobTaskAttribute>().ForEach(t =>
			{
				try
				{
					var attr = t.GetCustomAttribute<SchedulerJobTaskAttribute>();
					var job = _argonManager.Resolve(t) as IJobSchedulerTask;
					_logger.LogInformation(
						$"Adding Job {t.Name} StartNow {attr.StartNow} every {attr.Seconds} seconds");
					var schedule = registry.Schedule(() => job.Execute()).WithName(t.Name);

					if (attr.StartNow)
						schedule.ToRunNow().AndEvery(attr.Seconds).Seconds();
					else
						schedule.ToRunEvery(attr.Seconds).Seconds();

					_jobs.Add(job);
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error during add job {t.Name} => {ex}");
				}
			});



			return registry;
		}
	}
}

