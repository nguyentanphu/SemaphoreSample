using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SemaphoreSlimSample
{
	class Program
	{
		static async Task Main(string[] args)
		{
			int minWorkerThreadCount;
			int minIOThreadCount;
			int maxWorkerThreadCount;
			int maxIOThreadCount;

			ThreadPool.GetMinThreads(out minWorkerThreadCount, out minIOThreadCount);
			ThreadPool.GetMaxThreads(out maxWorkerThreadCount, out maxIOThreadCount);

			Console.WriteLine($"Min: {minWorkerThreadCount}-{minIOThreadCount}, Max: {maxWorkerThreadCount}-{maxIOThreadCount}");

			var result = await RunComputationsAsync(2, 10);

			Console.WriteLine($"Time passed: {result.Item2}, maxDegreeOfParallelism={1}");
		}

		static async Task<int> TaolaoTask(SemaphoreSlim semaphore)
		{
			try
			{
				await semaphore.WaitAsync();

				await Task.Delay(500);
				await Task.Delay(500);
			}
			finally
			{
				semaphore.Release();
			}
			return 1;
		}
		static async Task<(IReadOnlyCollection<int>, TimeSpan)> RunComputationsAsync(int maxDegreeOfParallelism, int messageCount)
		{
			SemaphoreSlim semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
			List<Task<int>> tasks = new List<Task<int>>();

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			for (int i = 0; i < messageCount; i++)
			{

					tasks.Add(TaolaoTask(semaphore));
				
			}
			await Task.WhenAll(tasks);

			stopwatch.Stop();
			return (tasks.Select(t => t.Result).ToImmutableList(), stopwatch.Elapsed);
		}
	}
}
